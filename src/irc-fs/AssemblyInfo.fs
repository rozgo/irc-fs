namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("irc-fs")>]
[<assembly: AssemblyProductAttribute("irc-fs")>]
[<assembly: AssemblyDescriptionAttribute("An IRC client library for F#")>]
[<assembly: AssemblyVersionAttribute("1.0.4")>]
[<assembly: AssemblyFileVersionAttribute("1.0.4")>]
[<assembly: GuidAttribute("694ab3b0-8929-4f78-ab72-55f29eb48a36")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.4"
