module IrcFS.Net

open IrcFS

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

type IrcClient private (server: string, port: int, client: TcpClient, data_stream: Stream) as _this = 
    let mutable disposed = false

    let msg_event = new Event<IrcMessage>()
    
    let reader = new StreamReader(data_stream) |> TextReader.Synchronized
    let writer = new StreamWriter(data_stream, AutoFlush = true)

    let msg_processor_active = ref false
    let msg_processor = MailboxProcessor<bool>.Start(fun inbox ->
        let rec loop enabled =
            async {
                if _this.Connected then 
                    match enabled with
                    | false ->
                        let! new_state = inbox.Receive()
                        msg_processor_active := new_state
                        do! loop new_state
                    | true ->
                        if inbox.CurrentQueueLength > 0 then
                            let! new_state = inbox.Receive()
                            msg_processor_active := new_state
                            do! loop new_state
                        else
                            let! message = reader.ReadLineAsync() |> Async.AwaitTask
                            msg_event.Trigger (IrcMessage.Parse(message))
                            do! loop true
            }
        loop false)

    do msg_processor.Error.Add(fun ex -> raise ex)

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
        msg_processor.Post true

    member this.StopEvent() =
        throwIfDisposed disposed
        msg_processor.Post false

    member this.ReadLine() = 
        throwIfDisposed disposed
        if !msg_processor_active then invalidOp "This operation cannot be performed while the MessageReceived event is active"

        reader.ReadLine()

    member this.ReadLineAsync() = 
        throwIfDisposed disposed
        if !msg_processor_active then invalidOp "This operation cannot be performed while the MessageReceived event is active"

        reader.ReadLineAsync() 
        |> Async.AwaitTask

    member this.WriteLine(line: string) = 
        throwIfDisposed disposed
        writer.WriteLine line
            
    member this.WriteLineAsync(line: string) = 
        throwIfDisposed disposed
        writer.WriteLineAsync line
        |> Async.AwaitIAsyncResult
        |> Async.Ignore
    
    member this.ReadMessage() = 
        this.ReadLine()
        |> IrcMessage.Parse

    member this.ReadMessageAsync() = 
        async { let! message = this.ReadLineAsync()
                return IrcMessage.Parse message }

    member this.WriteMessage(message: IrcMessage) = 
        message.ToString()
        |> this.WriteLine

    member this.WriteMessageAsync(message: IrcMessage) = 
        async { 
            do! this.WriteLineAsync(message.ToString())
        }

    interface IDisposable with
        member this.Dispose() = 
            do disposed <- true
               client.Close()
               this.StopEvent()
               dispose [ reader; writer; msg_processor ]

               match data_stream with
               | :? SslStream as sslStream -> (sslStream :> IDisposable).Dispose()
               | _ -> ()
