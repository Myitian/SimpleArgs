# SimpleArgs
a simple argument parser

```C#
ArgParser argx = new(args, ignoreCase: true,
    new("--help", 0, "-h", "-?"),
    new("--version", 0, "-v"),
    new("--string", 1, "-s")
    {
        Info = "receive string",
        Default = "HelloWorld"
    },
    new("--string3", 3, "-s3")
    {
        Info = "receive 3 strings"
    };

if (argx.Results.ContainsKey("--help"))
{
    argx.WriteHelp(Console.Out);
    return;
}
if (argx.TryGetString("--string", out string? s))
{
    Console.WriteLine("string:");
    Console.WriteLine(s);
}
if (argx.TryGetSpan("--string3", out ReadOnlySpan<string> s3))
{
    Console.WriteLine("string3:");
    foreach (string ss in s3)
        Console.WriteLine(ss);
}
```