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

    type public Targets =
        | Traditional = 1
        | Simplified = 2
        | Pinyin = 4
        | English = 8
        | All = 15

    type public Match =
        | Partial = 0
        | Full = 1

    type public SearchOptions = {
        Targets : Targets
        Match : Match
        Limit : int
    }

    [<Sealed>]
    type public Dict private (entries : Entry seq) =

        let (|>>) f g x = f x || g x

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
        member public this.Length = entries.Count ()

        static member public FromStream (stream : Stream) =
            use reader = new StreamReader (stream)

            reader.ReadToEnd().Split '\n'
                |> Array.toList
                |> readEntries []
                |> Dict

        static member public FromFile path =
            File.OpenRead path |> Dict.FromStream

        member public this.Search (options, value) =
            let isMatch =
                if options.Match = Match.Partial then
                    (fun (x : string) -> x.Contains value)
                else (=) value

            let predicates = seq {
                if options.Targets.HasFlag Targets.Traditional then
                    yield (fun entry -> isMatch entry.Traditional)
                if options.Targets.HasFlag Targets.Simplified then
                    yield (fun entry -> isMatch entry.Simplified)
                if options.Targets.HasFlag Targets.Pinyin then
                    yield (fun entry -> isMatch entry.Pinyin)
                if options.Targets.HasFlag Targets.English then
                    yield (fun entry -> Seq.exists isMatch entry.English)
            }

            let predicate = Seq.reduce (|>>) predicates
            let matches = Seq.filter predicate entries

            matches.Take options.Limit
