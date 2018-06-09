module DictTests

    open Cedict
    open FsUnit
    open System
    open System.IO
    open System.Text
    open Xunit

    let createEntries (lines : string list) =
        String.Join (Environment.NewLine, lines)
            |> Encoding.UTF8.GetBytes
            |> (fun buffer -> new MemoryStream (buffer))
            |> Dict.fromStream

    let parseTarget = function
        | "Traditional" -> Traditional
        | "Simplified" -> Simplified
        | "Pinyin" -> Pinyin
        | "English" -> English
        | _ -> ArgumentOutOfRangeException () |> raise

    [<Fact>]
    let ``Cedict.fromStream should return entries`` () =
        createEntries []
            |> should not' (be null)

    [<Fact>]
    let ``Cedict.fromStream should return empty entries when there are no entries`` () =
        createEntries []
            |> should haveLength 0

    [<Fact>]
    let ``Cedict.fromStream should not read invalid entries`` () =
        createEntries [""; Environment.NewLine; "Hello"; "你好"]
            |> should haveLength 0

    [<Fact>]
    let ``Cedict.fromStream should read valid entries`` () =
        createEntries ["你好 你好 [ni3 hao3] /Hello!/Hi!/How are you?/"]
            |> should haveLength 1

    [<Fact>]
    let ``Cedict.fromStream should parse valid entries`` () =
        let entries = createEntries ["你好 你好 [ni3 hao3] /Hello!/Hi!/How are you?/"];
        let expected = {
            Traditional = "你好"
            Simplified = "你好"
            Pinyin = "ni3 hao3"
            English = [|"Hello!"; "Hi!"; "How are you?"|]
        }
        List.exactlyOne entries
            |> should equal expected

    [<Theory>]
    [<InlineData ("Traditional", "中國")>]
    [<InlineData ("Simplified", "中国")>]
    [<InlineData ("Pinyin", "Zhong1 guo2")>]
    [<InlineData ("English", "China")>]
    let ``Cedict.search should not return results for target`` target value =
        let entries = createEntries ["中國 中国 [Zhong1 guo2] /China/"]
        let notTarget = parseTarget target |> (<>)
        let targets = List.filter notTarget [Traditional; Simplified; Pinyin; English]
        let options = {
            Targets = targets
            Match = Match.Full
            Limit = 1
        }
        Dict.search options value entries
            |> List.length
            |> should equal 0

    [<Theory>]
    [<InlineData ("Traditional", "中國")>]
    [<InlineData ("Simplified", "中国")>]
    [<InlineData ("Pinyin", "Zhong1 guo2")>]
    [<InlineData ("English", "China")>]
    let ``Cedict.search should return results for target`` target value =
        let entries = createEntries ["中國 中国 [Zhong1 guo2] /China/"]
        let targets = [parseTarget target]
        let options = {
            Targets = targets
            Match = Match.Full
            Limit = 1
        }
        Dict.search options value entries
            |> List.length
            |> should equal 1

    [<Theory>]
    [<InlineData ("Traditional", "國")>]
    [<InlineData ("Simplified", "国")>]
    [<InlineData ("Pinyin", "guo2")>]
    [<InlineData ("English", "Ch")>]
    let ``Cedict.search should not return results for full match`` target value =
        let entries = createEntries ["中國 中国 [Zhong1 guo2] /China/"]
        let tagets = [parseTarget target]
        let options = {
            Targets = tagets
            Match = Match.Full
            Limit = 1
        }
        Dict.search options value entries
            |> List.length
            |> should equal 0

    [<Theory>]
    [<InlineData ("Traditional", "國")>]
    [<InlineData ("Simplified", "国")>]
    [<InlineData ("Pinyin", "guo2")>]
    [<InlineData ("English", "Ch")>]
    let ``Cedict.search should return results for partial match`` target value =
        let entries = createEntries ["中國 中国 [Zhong1 guo2] /China/"]
        let targets = [parseTarget target]
        let options = {
            Targets = targets
            Match = Match.Partial
            Limit = 1
        }
        Dict.search options value entries
            |> List.length
            |> should equal 1

    [<Theory>]
    [<InlineData "中國">]
    [<InlineData "中国">]
    [<InlineData "Zhong1 guo2">]
    [<InlineData "China">]
    let ``Cedict.search should limit results`` value =
        let entries = createEntries ["中國 中国 [Zhong1 guo2] /China/"; "中國 中国 [Zhong1 guo2] /China/"]
        let options = {
            Targets = [Traditional; Simplified; Pinyin; English]
            Match = Match.Full
            Limit = 1
        }
        Dict.search options value entries
            |> List.length
            |> should equal 1
