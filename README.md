# Cedict

Library for reading CEDICT Chinese to English dictionary.

## Getting started

Install the NuGet package into your application.

### Package Manager

```shell
Install-Package Cedict
```

### .NET CLI

```shell
dotnet add package Cedict
```

## Usage

### Read entries from a CC-CEDICT database

```fsharp
let entries = Dict.fromFile "/path/to/cedict_ts.u8"
```

### Search Simplified Chinese for the exact text "中国", limited to 1 result

```fsharp
let options = {
    Targets = [Simplified]
    Match = Match.Full
    Limit = 1
}

Dict.search options "中国" entries

// Traditional = "中國"
// Simplified = "中国"
// Pinyin = "Zhong1 guo2"
// English = [|"China"|]
```

### Search Simplified Chinese and Traditional Chinese for any text containing "好", limited to 5 results

```fsharp
let options = {
    Targets = [Traditional; Simplified]
    Match = Match.Partial
    Limit = 5
}

Dict.search options "好" entries

// Traditional = "一把好手"
// Simplified = "一把好手"
// Pinyin = "yi1 ba3 hao3 shou3"
// English = [|"expert"; "dab hand"|]
//
// Traditional = "上好"
// Simplified = "上好"
// Pinyin = "shang4 hao3"
// English = [|"first-rate"; "top-notch"|]
//
// ...
```

### Search English for any text containing "China", limited to 5 results

```fsharp
let options = {
    Targets = [English]
    Match = Match.Partial
    Limit = 5
}

Dict.search options "China" entries

// Traditional = "3C"
// Simplified = "3C"
// Pinyin = "san1 C"
// English = [|"abbr. for computers, communications, and consumer electronics"; "China Compulsory Certificate (CCC)"|]
//
// Traditional = "䴉嘴鷸"
// Simplified = "鹮嘴鹬"
// Pinyin = "huan2 zui3 yu4"
// English = [|"(bird species of China) ibisbill (Ibidorhyncha struthersii)"|];}
//
// ...
```
