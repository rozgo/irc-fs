namespace IrcFs

open System.Text
open Microsoft.FSharp.Core.Printf

type IrcRecipient = 
    | Channel of chstring: string
    | User of nick: string * user: string option * host: string option
    | Server of host: string

    override this.ToString() = 
        match this with
        | Channel chstr -> sprintf "#%s" chstr
        | User(nick, user, host) -> 
            let user = Option.fold (fun _ u -> "!" + u) "" user
            let host = Option.fold (fun _ h -> "@" + h) "" host
            sprintf "%s%s%s" nick user host
        | Server hostname -> hostname

type IrcMessage = 
    | IrcMessage of prefix: IrcRecipient option * cmd: string * args: string list
    
    override this.ToString() = 
        let concat_irc_args args = 
            let sb = new StringBuilder()
            let rec loop args = 
                match args with
                | [ single_arg ] -> 
                    bprintf sb ":%s" single_arg
                    sb.ToString()
                | arg :: rest -> 
                    bprintf sb "%s " arg
                    loop rest
                | [] -> sb.ToString()
            loop args

        match this with
        | IrcMessage(prefix, cmd, args) -> 
            let prefix = Option.fold (fun _ pfx -> sprintf ":%O " pfx) "" prefix
            sprintf "%s%s %s" prefix cmd (concat_irc_args args)
    
    member this.Prefix = 
        match this with
        | IrcMessage(prefix, _, _) -> prefix
    
    member this.Command = 
        match this with
        | IrcMessage(_, cmd, _) -> cmd
    
    member this.Arguments = 
        match this with
        | IrcMessage(_, _, args) -> args
