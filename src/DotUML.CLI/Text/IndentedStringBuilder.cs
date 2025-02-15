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

    public IndentedStringBuilder Append(string text)
    {
        var lines = text.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            _builder.AppendLine($"{new string(' ', IndentLevel * _indentString.Length)}{line}");
        }
        return this;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}
