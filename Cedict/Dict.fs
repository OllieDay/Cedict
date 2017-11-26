namespace Cedict

    open System.IO
    open System.Linq
    open System.Text.RegularExpressions

    [<Sealed>]
    type public Entry internal (traditional, simplified, pinyin, english) =

        member public this.Traditional = traditional
        member public this.Simplified = simplified
        member public this.Pinyin = pinyin
        member public this.English = english

    [<Sealed>]
    type public Dict private (entries : Entry seq) =

        static let readEntry line =
            let matches = Regex.Matches (line, "(\S+)\s(\S+)\s\[([^\]]+)\]\s\/(.+)\/")

            if matches.Count = 1 then
                let groups = matches.[0].Groups

                Some <| Entry (
                    groups.[1].Value,
                    groups.[2].Value,
                    groups.[3].Value,
                    Array.toSeq <| groups.[4].Value.Split '/'
                )
            else None

        static let rec readEntries entries = function
            | [] -> entries
            | head :: tail ->
                let newEntries =
                    match readEntry head with
                    | Some entry -> entry :: entries
                    | None -> entries

                readEntries newEntries tail

        member public this.Entries = entries
        member public this.Length = entries.Count()

        static member public FromStream (stream : Stream) =
            use reader = new StreamReader (stream)

            reader.ReadToEnd().Split '\n'
                |> Array.toList
                |> readEntries []
                |> List.toSeq
                |> Dict

        static member public FromFile path =
            File.OpenRead path |> Dict.FromStream
