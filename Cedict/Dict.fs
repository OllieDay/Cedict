namespace Cedict

    open System.IO
    open System.Linq
    open System.Text.RegularExpressions

    type public Entry = {
        Traditional : string
        Simplified : string
        Pinyin : string
        English : string seq
    }


    [<Sealed>]
    type public Dict private (entries : Entry seq) =

        static let readEntry line =
            let matches = Regex.Matches (line, "(\S+)\s(\S+)\s\[([^\]]+)\]\s\/(.+)\/")

            if matches.Count = 1 then
                let groups = matches.[0].Groups

                Some {
                    Traditional = groups.[1].Value
                    Simplified = groups.[2].Value
                    Pinyin = groups.[3].Value
                    English = groups.[4].Value.Split '/'
                }
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
                |> Dict

        static member public FromFile path =
            File.OpenRead path |> Dict.FromStream
