module DictTests

    open Cedict
    open FsUnit
    open System
    open System.IO
    open System.Text
    open Xunit

    let createDict (lines : string list) =
        String.Join (Environment.NewLine, lines)
            |> Encoding.UTF8.GetBytes
            |> (fun buffer -> new MemoryStream (buffer))
            |> Dict.FromStream

    [<Fact>]
    let ``Dict.FromStream should return Dict`` () =
        createDict []
            |> should not' (be null)

    [<Fact>]
    let ``Dict.FromStream should return empty Dict when there are no entries`` () =
        createDict []
            |> should haveLength 0

    [<Fact>]
    let ``Dict.FromStream should not read invalid entries`` () =
        createDict [""; Environment.NewLine; "Hello"; "你好"]
            |> should haveLength 0

    [<Fact>]
    let ``Dict.FromStream should read valid entries`` () =
        createDict ["你好 你好 [ni3 hao3] /Hello!/Hi!/How are you?/"]
            |> should haveLength 1

    [<Fact>]
    let ``Dict.FromStream should parse valid entries`` () =
        let dict = createDict ["你好 你好 [ni3 hao3] /Hello!/Hi!/How are you?/"];

        let expected = {
            Traditional = "你好"
            Simplified = "你好"
            Pinyin = "ni3 hao3"
            English = [|"Hello!"; "Hi!"; "How are you?"|]
        }

        Seq.exactlyOne dict.Entries
            |> should equal expected

    [<Theory>]
    [<InlineData (Targets.Traditional, "中國")>]
    [<InlineData (Targets.Simplified, "中国")>]
    [<InlineData (Targets.Pinyin, "Zhong1 guo2")>]
    [<InlineData (Targets.English, "China")>]
    let ``Dict.Search should not return results for target`` target value =
        let dict = createDict ["中國 中国 [Zhong1 guo2] /China/"]

        let targets = Targets.All &&& ~~~target

        let options = {
            Targets = targets
            Match = Match.Full
            Limit = 1
        }

        let results = dict.Search (options, value)

        Seq.length results
            |> should equal 0

    [<Theory>]
    [<InlineData (Targets.Traditional, "中國")>]
    [<InlineData (Targets.Simplified, "中国")>]
    [<InlineData (Targets.Pinyin, "Zhong1 guo2")>]
    [<InlineData (Targets.English, "China")>]
    let ``Dict.Search should return results for target`` target value =
        let dict = createDict ["中國 中国 [Zhong1 guo2] /China/"]

        let options = {
            Targets = target
            Match = Match.Full
            Limit = 1
        }

        let results = dict.Search (options, value)

        Seq.length results
            |> should equal 1

    [<Theory>]
    [<InlineData (Targets.Traditional, "國")>]
    [<InlineData (Targets.Simplified, "国")>]
    [<InlineData (Targets.Pinyin, "guo2")>]
    [<InlineData (Targets.English, "Ch")>]
    let ``Dict.Search should return results for full match`` target value =
        let dict = createDict ["中國 中国 [Zhong1 guo2] /China/"]

        let options = {
            Targets = target
            Match = Match.Full
            Limit = 1
        }

        let results = dict.Search (options, value)

        Seq.length results
            |> should equal 0

    [<Theory>]
    [<InlineData (Targets.Traditional, "國")>]
    [<InlineData (Targets.Simplified, "国")>]
    [<InlineData (Targets.Pinyin, "guo2")>]
    [<InlineData (Targets.English, "Ch")>]
    let ``Dict.Search should return results for partial match`` target value =
        let dict = createDict ["中國 中国 [Zhong1 guo2] /China/"]

        let options = {
            Targets = target
            Match = Match.Partial
            Limit = 1
        }

        let results = dict.Search (options, value)

        Seq.length results
            |> should equal 1

    [<Theory>]
    [<InlineData "中國">]
    [<InlineData "中国">]
    [<InlineData "Zhong1 guo2">]
    [<InlineData "China">]
    let ``Dict.Search should limit results`` value =
        let dict = createDict ["中國 中国 [Zhong1 guo2] /China/"; "中國 中国 [Zhong1 guo2] /China/"]

        let options = {
            Targets = Targets.All
            Match = Match.Full
            Limit = 1
        }

        let results = dict.Search (options, value)

        Seq.length results
            |> should equal 1
