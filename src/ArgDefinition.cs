namespace SimpleArgs;

public sealed class ArgDefinition(string name, int paramCount, params string[] aliases)
{
    public readonly string Name = name;
    public readonly string[] Aliases = aliases;
    public readonly int ParamCount = paramCount;
    public string? Default;
    public string? Info;
}
