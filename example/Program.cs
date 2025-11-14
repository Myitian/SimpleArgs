using SimpleArgs;
using System.Reflection;

const string ArgHelp = "--help";
const string ArgVersion = "--version";
const string ArgLenientBool = "--lenient-bool";
const string ArgStrictBool = "--strict-bool";
const string ArgByte = "--byte";
const string ArgInt = "--int";
const string ArgLong = "--long";
const string ArgString = "--string";
const string ArgSpan3 = "--span3";
const string ArgList = "--list";


ArgParser argx = new(args, true,
    new(ArgHelp, 0, "-h", "-?"),
    new(ArgVersion, 0, "-v"),
    new(ArgLenientBool, 1, "-lb")
    {
        Info = "receive boolean, true=yes=1, false=no=0"
    },
    new(ArgStrictBool, 1, "-sb")
    {
        Info = "receive boolean, true/false"
    },
    new(ArgByte, 1, "-b")
    {
        Info = "receive uint8"
    },
    new(ArgInt, 1, "-i")
    {
        Info = "receive int32",
        Default = "12345"
    },
    new(ArgLong, 1, "-l")
    {
        Info = "receive int64"
    },
    new(ArgString, 1, "-s")
    {
        Info = "receive string"
    },
    new(ArgSpan3, 3, "-s3")
    {
        Info = "receive 3 strings"
    },
    new(ArgList, 1)
    {
        Info = "receive string list"
    });

if (argx.Results.ContainsKey(ArgHelp))
{
    argx.WriteHelp(Console.Out);
    return;
}
if (argx.Results.ContainsKey(ArgVersion))
{
    Console.WriteLine(Assembly.GetExecutingAssembly().GetName());
    return;
}

Console.WriteLine($"{ArgLenientBool}:");
if (argx.TryGetBoolean(ArgLenientBool, out bool lb))
    Console.WriteLine(lb);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgStrictBool}:");
if (argx.TryGet(ArgStrictBool, out bool sb))
    Console.WriteLine(sb);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgByte}:");
if (argx.TryGet(ArgByte, out byte b))
    Console.WriteLine(b);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgInt}:");
if (argx.TryGet(ArgInt, out int i))
    Console.WriteLine(i);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgLong}:");
if (argx.TryGet(ArgLong, out int l))
    Console.WriteLine(l);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgString}:");
if (argx.TryGetString(ArgString, out string? s))
    Console.WriteLine(s);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgSpan3}:");
if (argx.TryGetSpan(ArgSpan3, out ReadOnlySpan<string> s3))
    foreach (string ss in s3)
        Console.WriteLine(ss);
else
    Console.WriteLine("Not Exists / Format Error");
Console.WriteLine($"{ArgList}:");
if (argx.TryGetFullList(ArgList, out List<string>? list))
    foreach (string ss in list)
        Console.WriteLine(ss);
else
    Console.WriteLine("Not Exists / Format Error");

Console.WriteLine($"UnknownArgs: {argx.UnknownArgs.Count}");
foreach (string ss in argx.UnknownArgs)
    Console.WriteLine(ss);