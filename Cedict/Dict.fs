namespace Cedict

    type Entry = {
        Traditional : string
        Simplified : string
        Pinyin : string
        English : seq<string>
    }

    type Target = Traditional | Simplified | Pinyin | English
    type Match = Partial | Full

    type SearchOptions = {
        Targets : Target list
        Match : Match
        Limit : int
    }

    module Dict =

        open System.IO
        open System.Text.RegularExpressions

        let private readLines (stream : Stream) =
            use reader = new StreamReader (stream)
            let rec readLines lines =
                match reader.ReadLine () with
                | null -> lines
                | line -> readLines (line :: lines)
            readLines []

        let private readEntry line =
            let matches = Regex.Matches (line, "(\S+)\s(\S+)\s\[([^\]]+)\]\s\/(.+)\/")
            match matches.Count with
            | 1 ->
                let groups = matches.[0].Groups
                Some {
                    Traditional = groups.[1].Value
                    Simplified = groups.[2].Value
                    Pinyin = groups.[3].Value
                    English = groups.[4].Value.Split '/'
                }
            | _ -> None

        let private readEntries =
            List.map readEntry
                >> List.choose id

        let fromStream : Stream -> Entry list =
            readLines >> readEntries

        let fromFile =
            File.OpenRead >> fromStream

        let search options value =
            let isExactMatch = (=) value
            let isPartialMatch (x : string) = x.Contains value
            let isMatch =
                match options.Match with
                | Full -> isExactMatch
                | Partial -> isPartialMatch
            let hasTarget target = List.contains target options.Targets
            let predicates = seq {
                if hasTarget Traditional then
                    yield (fun entry -> isMatch entry.Traditional)
                if hasTarget Simplified then
                    yield (fun entry -> isMatch entry.Simplified)
                if hasTarget Pinyin then
                    yield (fun entry -> isMatch entry.Pinyin)
                if hasTarget English then
                    yield (fun entry -> Seq.exists isMatch entry.English)
            }
            let either f g x = f x || g x
            let predicate = Seq.reduce either predicates
            List.filter predicate >> List.truncate options.Limit
