using System.Text;

namespace DotUML.CLI.Text;

public class IndentedStringBuilder
{
    private readonly StringBuilder _builder = new StringBuilder();
    public int IndentLevel { get; private set; } = 0;
    private readonly string _indentString;
    public IndentedStringBuilder(string indentString = "    ")
    {
        _indentString = indentString;
    }

    public IndentedStringBuilder IncreaseIndent()
    {
        IndentLevel++;
        return this;
    }

    public IndentedStringBuilder DecreaseIndent()
    {
        if (IndentLevel > 0)
            IndentLevel--;
        return this;
    }

    public IndentedStringBuilder AppendLine(string text)
    {
        _builder.AppendLine($"{new string(' ', IndentLevel * _indentString.Length)}{text}");
        return this;
    }

    public IndentedStringBuilder AppendJoin(string separator, IEnumerable<string> values)
    {
        _builder.Append($"{new string(' ', IndentLevel * _indentString.Length)}{string.Join(separator, values)}");
        return this;
    }

    public IndentedStringBuilder Append(string text)
    {
        _builder.Append($"{new string(' ', IndentLevel * _indentString.Length)}{text}");
        return this;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}

