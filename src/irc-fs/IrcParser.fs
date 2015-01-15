namespace IrcFS

open FParsec.CharParsers
open FParsec.Primitives

open IrcFS

module internal Parser =
    
    module private Internal = 
        let word = many1Satisfy ((<>) ' ')

        let letter = asciiLetter
        
        // correct irc grammar, but causes Failure result on malformed nick
        // let number   = digit
        // let special  = anyOf @"-[]\`^{}"
        // let nick = many1Satisfy2 (isLetter) (choice [letter; number; special;]) .>> nextCharSatisfiesNot ((=) '.')

        let nick = many1Satisfy2 (isLetter) (isNoneOf "!@. ") .>> nextCharSatisfiesNot ((=) '.')

        let p_user_segment = skipChar '!' >>. many1Satisfy(isNoneOf "@ ")
        let p_host_segment = skipChar '@' >>. word
        let p_user = tuple3 (nick) (opt p_user_segment) (opt p_host_segment)

        let user_recipient = p_user |>> User
        let channel_recipient = many1Chars2 (anyOf "#&+!") (noneOf "\x00\a\r\n, ")
                                |>> Channel
        let server_recipient = word |>> Server

        let recipient = choice [|attempt channel_recipient; attempt user_recipient; attempt server_recipient|]
        let recipients = sepBy1 recipient (skipChar ',') .>> spaces1

        let irc_prefix = skipChar ':' >>. recipient .>> spaces1

        let p_tail_param = skipChar ':' >>. restOfLine false
        let p_middle_param = nextCharSatisfies ((<>) ':') >>. word .>> spaces
        let irc_command = many1Chars letter <|> manyMinMaxSatisfy 3 3 isDigit .>> spaces1
        let param_array = many (p_middle_param <|> p_tail_param)

        let message =
            tuple3 (opt irc_prefix) irc_command param_array |>> IrcMessage

    let internal tryUnboxParserResult result = 
        match result with
        | ParserResult.Success(r, _, _) -> Some r
        | _ -> None

    let internal unboxParserResult result =
        match result with
        | ParserResult.Success(r, _, _) -> r
        | _ -> invalidArg "result" "The supplied parser result was Failure"
        
    let parseIrcMessage str =
        run Internal.message str

    let parseRecipient str =
        run Internal.recipient str

    let parseRecipients str =
        run Internal.recipients str
