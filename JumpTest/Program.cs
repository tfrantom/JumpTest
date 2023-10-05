string[] lines = args switch {
    ["-f", string name, ..] => File.ReadAllLines(name),
    [string blob] => blob.Split('\n'),
    _ => new[] {"BAD ARGUMENTS"}
};
var isValidString = (string s) => s.Any(c => !(c is '+' or '<' or '`'));

var getCode = (string s) =>
{
    var underscore = s.IndexOf('_');
    s = s.Substring(0, underscore > 0 ? underscore : s.Length);
    var paren = s.IndexOf('(');
    s = s.Substring(0, paren > 0 ? paren : s.Length);
    var lt = s.IndexOf('<');
    s = s.Substring(0, lt > 0 ? lt : s.Length);
    var bracket = s.IndexOf('[');
    s = s.Substring(0, bracket > 0 ? bracket : s.Length);
    return s.Trim('.');
};

var codeLines = lines
    .Select(
        (line, i) => //Generate lines of valid c# so that lsp jumping works in this file.
            line.Trim().Split(' ') switch
            {
                ["at" or "Failed" or "Skipped", string c, ..] when isValidString(c)
                    => ($"using {getCode(c)};", $"\n//Trace {i + 1}: {line}\n"),
                [string c, ":", ..] 
                    => ($"using {getCode(c)};", $"\n//Trace {i + 1}: {line}\n"),
                [..] e => ("", $"{i}"),
                _ => ($"//LINE {i + 1}.Trim().Split(' ')", "")
            }
    )
    .Aggregate(
        "",
        (acc, data) =>
            (
                data is (string code, string trace)
                && !(code is null or "" or " " or "\n")
                && trace is not null
            )
                ? $"{acc}\n{code + trace}"
                : $"{acc}\n//MALFORMED CODE FROM : {data.Item2}"
    );
//FOR FILE WRITE IN CODE
//await File.WriteAllTextAsync("TestJumper.cs", codeLines);
//FOR *IX
Console.Write(codeLines);
return 0;
