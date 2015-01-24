# irc-fs [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/6nviekmy11m43dpa)](https://ci.appveyor.com/project/JahTheDev/irc-fs) [![Travis build status](https://travis-ci.org/JahTheDev/irc-fs.png)](https://travis-ci.org/JahTheDev/irc-fs)

irc-fs is an IRC client library for F#. It provides a simple framework for connecting to IRC servers, sending and receiving standard IRC messages defined in RFC 2812.

### Installation

Build irc-fs from the provided .sln file, `build.cmd` or `build.sh`. The solution is configured for .NET 4.5.3 and F# 4.0 by default.

### Getting Started

irc-fs supports two main modes of operation: synchronous and event-based. An `IrcClient` can be toggled between the modes using `IrcClient.StartEvent` and `IrcClient.StopEvent`. Receiving messages synchronously will not work when the event is active. A basic client might look like the following:

```fsharp
#r "IrcFS.dll"

open IrcFS

let channels = ["#channel"; "#channel2"]
use client = new Net.IrcClient("irc.example.com", 6667)

do client.WriteMessage (IrcMessage.nick "nickname")
   client.WriteMessage (IrcMessage.user "username" "0" "real name")

// an example of processing messages synchronously
let process_motd () =
    let rec loop () =
		match client.ReadMessage () with
		// assume it's safe to join channels after RPL_ENDOFMOTD or ERR_NOMOTD
		| IrcMessage(_, ResponseCode.RPL_ENDOFMOTD, _)
		| IrcMessage(_, ResponseCode.ERR_NOMOTD, _) ->
			client.WriteMessage (IrcMessage.join channels)
		| IrcMessage(_, "PING", value) -> 
			client.WriteMessage (IrcMessage.pong (List.exactlyOne value))
			loop ()
		| _ -> loop ()
	loop ()

// an example of event processing style
// responding to PING messages is the minimum requirement for a client
client.MessageReceived
|> Event.filter(fun message -> if message.Command = "PING" then true else false)
|> Event.add(fun message -> IrcMessage.pong (List.exactlyOne message.Arguments)
							|> client.WriteMessage)

do process_motd()
   client.StartEvent()
```

### License

irc-fs is available under the MIT license. For more information, see the [license file](https://github.com/JahTheDev/irc-fs/blob/master/LICENSE.md).