using System.Diagnostics.CodeAnalysis;

namespace SimpleArgs;

public sealed class ArgParser
{
    private readonly string[] args;
    private readonly ArgDefinition[] defs;
    public bool IgnoreCase { get; }
    public Dictionary<string, string?> DefaultMap { get; }
    public Dictionary<string, List<Range>> Results { get; }
    public List<string> UnknownArgs { get; } = [];
    public ArgParser(string[] args, bool ignoreCase, params ArgDefinition[] defs)
    {
        this.args = args;
        this.defs = defs;
        IgnoreCase = ignoreCase;
        StringComparer comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        Dictionary<string, ArgDefinition> argMap = new(comparer);
        DefaultMap = new(comparer);
        Results = new(comparer);
        foreach (ArgDefinition def in defs)
        {
            argMap[def.Name] = def;
            if (def.Default is not null)
                DefaultMap[def.Name] = def.Default;
            foreach (string alias in def.Aliases)
                argMap[alias] = def;
        }
        for (int i = 0; i < args.Length;)
        {
            string arg = args[i];
            if (!argMap.TryGetValue(arg, out ArgDefinition? argDef))
            {
                UnknownArgs.Add(arg);
                i++;
                continue;
            }
            int s = ++i;
            int e = i += argDef.ParamCount;
            if (e > args.Length)
            {
                UnknownArgs.Add(arg);
                UnknownArgs.AddRange(args.AsSpan(s));
                break;
            }
            if (Results.TryGetValue(arg, out List<Range>? ranges))
                ranges.Add(s..e);
            else
                Results[argDef.Name] = [s..e];
        }
    }
    public bool TryGetFullList([NotNullWhen(true)] string? name, [NotNullWhen(true)] out List<string>? value)
    {
        if (name is not null
            && Results.TryGetValue(name, out List<Range>? ranges))
        {
            value = [];
            foreach (Range range in ranges)
            {
                if (range.GetOffsetAndLength(args.Length) is (int offset, int length) && offset <= args.Length)
                    value.AddRange(args.AsSpan(offset, Math.Min(args.Length - offset, length)));
            }
            return true;
        }
        value = default;
        return false;
    }
    public bool TryGetSpan([NotNullWhen(true)] string? name, out ReadOnlySpan<string> value)
    {
        if (name is not null
            && Results.TryGetValue(name, out List<Range>? ranges)
            && ranges is [.., Range range]
            && range.GetOffsetAndLength(args.Length) is (int offset, int length)
            && offset <= args.Length)
        {
            value = args.AsSpan(offset, Math.Min(args.Length - offset, length));
            return true;
        }
        value = default;
        return false;
    }
    public bool TryGetString([NotNullWhen(true)] string? name, [NotNullWhen(true)] out string? value)
    {
        if (name is null)
        {
            value = default;
            return false;
        }
        if (TryGetSpan(name, out ReadOnlySpan<string> span)
            && !span.IsEmpty)
        {
            value = span[0];
            return true;
        }
        return DefaultMap.TryGetValue(name, out value);
    }
    public bool TryGet<T>([NotNullWhen(true)] string? name, [NotNullWhen(true)] out T? value, IFormatProvider? provider = null) where T : IParsable<T>
    {
        if (TryGetString(name, out string? s) && T.TryParse(s, provider, out value))
            return true;
        value = default;
        return false;
    }
    public bool TryGetEnum<TEnum>([NotNullWhen(true)] string? name, out TEnum value) where TEnum : struct, Enum
    {
        if (TryGetString(name, out string? s) && Enum.TryParse(s, IgnoreCase, out value))
            return true;
        value = default;
        return false;
    }
    public bool TryGetBoolean([NotNullWhen(true)] string? name, out bool value)
    {
        StringComparison comparison = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (TryGetString(name, out string? s))
        {
            if (string.IsNullOrEmpty(s))
                value = false;
            else if (long.TryParse(s, out long i))
                value = i != 0;
            else if ("yes".StartsWith(s, comparison) || "true".StartsWith(s, comparison))
                value = true;
            else if ("no".StartsWith(s, comparison) || "false".StartsWith(s, comparison))
                value = false;
            else
            {
                value = default;
                return false;
            }
            return true;
        }
        value = default;
        return false;
    }
    public void WriteHelp(TextWriter writer)
    {
        bool hasDefault = DefaultMap.Count != 0;
        int nLen = "Name".Length, cLen = "ParamCount".Length, aLen = "Alias".Length, dLen = "Default".Length;
        foreach (ArgDefinition def in defs)
        {
            nLen = Math.Max(nLen, def.Name.Length);
            cLen = Math.Max(cLen, def.ParamCount.ToString().Length);
            aLen = Math.Max(aLen, def.Aliases.Sum(a => a.Length) + Math.Max(def.Aliases.Length - 1, 0) * ", ".Length);
            if (hasDefault)
                dLen = Math.Max(dLen, def.Default?.Length ?? 0);
        }
        writer.WriteLine("Arguments:");
        writer.WritePadLeft("Name", nLen);
        writer.Write(" | ");
        writer.WritePadLeft("ParamCount", cLen);
        writer.Write(" | ");
        writer.WritePadLeft("Alias", aLen);
        if (hasDefault)
        {
            writer.Write(" | ");
            writer.WritePadLeft("Default", dLen);
        }
        writer.Write(" | Info");
        foreach (ArgDefinition aDef in defs)
        {
            writer.WriteLine();
            writer.WritePadLeft(aDef.Name, nLen);
            writer.Write(" . ");
            writer.WritePadRight(aDef.ParamCount, cLen);
            writer.Write(" . ");
            writer.WritePadLeft(string.Join(", ", aDef.Aliases), aLen);
            if (hasDefault)
            {
                writer.Write(" . ");
                writer.WritePadLeft(aDef.Default ?? "", dLen);
            }
            writer.Write(" . ");
            writer.Write(aDef.Info);
        }
    }
}
file static class Extensions
{
    static readonly string spaces = new(' ', 256);
    public static void WritePadLeft<T>(this TextWriter writer, T? obj, int width)
    {
        string text = $"{obj}";
        writer.Write(text);
        writer.WriteSpaces(Math.Max(width - text.Length, 0));
    }
    public static void WritePadRight<T>(this TextWriter writer, T? obj, int width)
    {
        string text = $"{obj}";
        writer.WriteSpaces(Math.Max(width - text.Length, 0));
        writer.Write(text);
    }
    public static void WriteSpaces(this TextWriter writer, int count)
    {
        while (count > spaces.Length)
        {
            writer.Write(spaces);
            count -= spaces.Length;
        }
        writer.Write(spaces.AsSpan(0, count));
    }
}