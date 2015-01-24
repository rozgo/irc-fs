[<AutoOpen>]
[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module IrcFS.IrcMessage

type IrcRecipient with
    static member ParseMany(value) =
        (Parser.parseRecipients >> Parser.unboxParserResult) value

    static member TryParseMany(value) =
        (Parser.parseRecipients >> Parser.tryUnboxParserResult) value

    static member Parse(value) =
        (Parser.parseRecipient >> Parser.unboxParserResult) value

    static member TryParse(value) =
        (Parser.parseRecipient >> Parser.tryUnboxParserResult) value

type IrcMessage with
    static member Parse(value) =
        (Parser.parseIrcMessage >> Parser.unboxParserResult) value

    static member TryParse(value) =
        (Parser.parseIrcMessage >> Parser.tryUnboxParserResult) value

    static member pass password = IrcMessage(None, "PASS", [password])

    static member nick nickname = IrcMessage(None, "NICK", [nickname])

    static member user username mode realname = IrcMessage(None, "USER", [username; mode; "*"; realname])

    static member oper name password = IrcMessage(None, "OPER", [name; password])

    static member mode user modes = IrcMessage(None, "MODE", [user; modes])

    static member service nickname distribution ``type`` info = IrcMessage(None, "SERVICE", [nickname; "*"; distribution; ``type``; "*"; info])

    static member quit ?message = IrcMessage(None, "QUIT", Option.toList message)

    static member squit server comment = IrcMessage(None, "SQUIT", [server; comment])

    static member join channels = IrcMessage(None, "JOIN", [String.concat "," channels])

    static member join(channels, keys) =  IrcMessage(None, "JOIN", [String.concat "," channels; String.concat "," keys])

    static member part channels = IrcMessage(None, "PART", [String.concat "," channels])

    static member part(channels, message) = IrcMessage(None, "PART", [String.concat "," channels; message])

    static member topic channel = IrcMessage(None, "TOPIC", [channel])

    static member topic(channel, topic) = IrcMessage(None, "TOPIC", [channel; topic])

    static member names channels = IrcMessage(None, "NAMES", [String.concat "," channels])

    static member names(channels, target) = IrcMessage(None, "NAMES", String.concat "," channels :: Option.toList target)

    static member list channels = IrcMessage(None, "LIST",  [String.concat "," channels])

    static member list(channels, target) = IrcMessage(None, "LIST",  [String.concat "," channels; target])

    static member invite nickname channel = IrcMessage(None, "INVITE", [nickname; channel])

    static member kick channels users = IrcMessage(None, "KICK", [String.concat "," channels; String.concat "," users])

    static member privmsg recipients message = IrcMessage(None, "PRIVMSG", [String.concat "," recipients; message])

    static member notice msgtarget text = IrcMessage(None, "NOTICE", [msgtarget; text])

    static member motd ?target = IrcMessage(None, "MOTD", Option.toList target)

    static member lusers () = IrcMessage(None, "LUSERS", [])

    static member lusers mask = IrcMessage(None, "LUSERS", [mask])

    static member lusers (mask, target) = IrcMessage(None, "LUSERS", [mask; target])

    static member version ?target = IrcMessage(None, "VERSION", Option.toList target)

    static member stats () = IrcMessage(None, "STATS", [])

    static member stats query = IrcMessage(None, "STATS", [query])

    static member stats (query, target) = IrcMessage(None, "STATS", [query; target])

    static member links () = IrcMessage(None, "LINKS", [])

    static member links remote_server = IrcMessage(None, "LINKS", [remote_server])

    static member links (remote_server, server_mask) = IrcMessage(None, "LINKS", [remote_server; server_mask])

    static member time ?target = IrcMessage(None, "TIME", Option.toList target)

    static member connect (target_server, port) = IrcMessage(None, "CONNECT", [target_server; port])

    static member connect (target_server, port, remote_server) = IrcMessage(None, "CONNECT", [target_server; port; remote_server])

    static member trace ?target = IrcMessage(None, "TRACE", Option.toList target)

    static member admin ?target = IrcMessage(None, "ADMIN", Option.toList target)

    static member info ?target = IrcMessage(None, "INFO", Option.toList target)

    static member servlist () = IrcMessage(None, "SERVLIST", [])

    static member servlist mask = IrcMessage(None, "SERVLIST", [mask])

    static member servlist (mask, ``type``) = IrcMessage(None, "SERVLIST", [mask; ``type``])

    static member squery servicename text = IrcMessage(None, "SQUERY", [servicename; text])

    static member who mask = IrcMessage(None, "WHO", [mask])

    static member who (mask, operators_only) = IrcMessage(None, "WHO", mask :: if operators_only then ["o"] else [])

    static member whois mask = IrcMessage(None, "WHOIS", [mask])

    static member whois masks = IrcMessage(None, "WHOIS", [String.concat "," masks])

    static member whois (target, masks) = IrcMessage(None, "WHOIS", [target; String.concat "," masks])

    static member whowas nicknames = IrcMessage(None, "WHOWAS", [String.concat "," nicknames])

    static member whowas (nicknames, count) = IrcMessage(None, "WHOWAS", [String.concat "," nicknames; count])

    static member whowas (nicknames, count, target) = IrcMessage(None, "WHOWAS", [String.concat "," nicknames; count; target])

    static member kill nickname comment = IrcMessage(None, "KILL", [nickname; comment])

    static member ping server1 = IrcMessage(None, "PING", [server1])

    static member ping(server1, server2) = IrcMessage(None, "PING", [server1; server2])

    static member pong server1 = IrcMessage(None, "PONG", [server1])

    static member pong(server1, server2) = IrcMessage(None, "PONG", [server1; server2])
