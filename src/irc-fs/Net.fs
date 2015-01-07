module IrcFs.Net

open IrcFs

open System
open System.IO
open System.Net.Security
open System.Net.Sockets
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates
open System.Threading

let inline private objDisposed<'T> = raise <| ObjectDisposedException(typeof<'T>.FullName)
let inline private dispose(garbage: seq<IDisposable>) = garbage |> Seq.iter(fun disposable -> disposable.Dispose())

let inline private throwIfDisposed<'T> predicate = 
    if predicate then objDisposed<'T>

let private no_ssl_error_callback = new RemoteCertificateValidationCallback(fun _ _ _ errors -> errors = SslPolicyErrors.None)

type IrcClient private (server: string, port: int, client: TcpClient, data_stream: Stream) = 
    let mutable disposed = false

    let msg_event = new Event<IrcMessage>()
    let mutable  msg_event_cts = new CancellationTokenSource()
    let mutable msg_processor = None
    
    let reader = new StreamReader(data_stream) |> TextReader.Synchronized
    let writer = new StreamWriter(data_stream, AutoFlush = true)

    new(server: string, port: int, ?ssl: bool, ?validate_cert_callback: RemoteCertificateValidationCallback) = 
        let client = new TcpClient(server, port)
        
        let data_stream: Stream = 
            match defaultArg ssl false with
            | true -> 
                upcast new SslStream(client.GetStream(), true, defaultArg validate_cert_callback no_ssl_error_callback)
            | false -> upcast client.GetStream()
        if data_stream :? SslStream then 
            do (data_stream :?> SslStream)
                .AuthenticateAsClient(server, new X509CertificateCollection(), 
                                      SslProtocols.Default ||| SslProtocols.Tls12, true)
        new IrcClient(server, port, client, data_stream)
    
    static member ConnectAsync(server: string, port: int, ?ssl: bool, 
                               ?validate_cert_callback: RemoteCertificateValidationCallback) = 
        async { 
            let client = new TcpClient()
            do! client.ConnectAsync(server, port)
                |> Async.AwaitIAsyncResult
                |> Async.Ignore
            let data_stream: Stream = 
                match defaultArg ssl false with
                | true -> 
                    upcast new SslStream(client.GetStream(), true, 
                                         defaultArg validate_cert_callback no_ssl_error_callback)
                | false -> upcast client.GetStream()
            if data_stream :? SslStream then 
                do! (data_stream :?> SslStream)
                        .AuthenticateAsClientAsync(server, new X509CertificateCollection(), 
                                                   SslProtocols.Default ||| SslProtocols.Tls12, true)
                    |> Async.AwaitIAsyncResult
                    |> Async.Ignore
            return new IrcClient(server, port, client, data_stream)
        }
    member this.Server = server
    member this.Port = port
    
    [<CLIEvent>]
    member this.MessageReceived = msg_event.Publish
    
    member this.Connected = 
        throwIfDisposed disposed
        client.Client.Connected
    
    member this.StartEvent() = 
        throwIfDisposed disposed
        match msg_processor with
        | Some _ -> invalidOp "An event loop is already started"
        | None -> 
            msg_processor <- 
                MailboxProcessor<unit>.Start(fun inbox ->
                    async {
                       while this.Connected && not msg_event_cts.Token.IsCancellationRequested do
                            let! message = reader.ReadLineAsync() |> Async.AwaitTask
                            msg_event.Trigger (IrcMessage.Parse(message))
                    })
                |> Some

            msg_processor.Value.Error.Add(fun ex -> raise ex)

    member this.StopEvent() =
        throwIfDisposed disposed
        msg_event_cts.Cancel()
        Option.iter(fun agent -> (agent :> IDisposable).Dispose()) msg_processor
        msg_processor <- None
        msg_event_cts <- new CancellationTokenSource()

    member this.ReadLineAsync() = 
        throwIfDisposed disposed
        if msg_processor.IsSome then invalidOp "This operation cannot be performed while the MessageReceived event is active"

        reader.ReadLineAsync() |> Async.AwaitTask
    
    member this.WriteLineAsync(line: string) = 
        throwIfDisposed disposed

        writer.WriteLineAsync(line)
        |> Async.AwaitIAsyncResult
        |> Async.Ignore
    
    member this.ReadLine() = 
        this.ReadLineAsync() 
        |> Async.RunSynchronously

    member this.WriteLine(line: string) = 
        this.WriteLineAsync(line)
        |> Async.RunSynchronously

    member this.ReadMessageAsync() = 
        async { let! message = this.ReadLineAsync()
                return IrcMessage.Parse message }

    member this.WriteMessageAsync(message: IrcMessage) = 
        async { 
            do! this.WriteLineAsync(message.ToString())
        }

    member this.ReadMessage() = 
        this.ReadMessageAsync() 
        |> Async.RunSynchronously

    member this.WriteMessage(message: IrcMessage) = 
        this.WriteMessageAsync(message)
        |> Async.RunSynchronously

    interface IDisposable with
        member this.Dispose() = 
            do disposed <- true
               client.Close()
               dispose [ reader; writer; ]
               Option.iter(fun agent -> (agent :> IDisposable).Dispose()) msg_processor

               match data_stream with
               | :? SslStream as sslStream -> (sslStream :> IDisposable).Dispose()
               | _ -> ()
