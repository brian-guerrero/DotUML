using System;

namespace DotUML.CLI.Mermaid;

using System.Text;

public class IndentedStringBuilder
{
    private readonly StringBuilder _builder = new StringBuilder();
    private int _indentLevel;
    private readonly string _indentString;
    public IndentedStringBuilder(string indentString = "    ")
    {
        _indentString = indentString;
    }

    public IndentedStringBuilder IncreaseIndent()
    {
        _indentLevel++;
        return this;
    }

    public IndentedStringBuilder DecreaseIndent()
    {
        if (_indentLevel > 0)
            _indentLevel--;
        return this;
    }

    public IndentedStringBuilder AppendLine(string text)
    {
        _builder.AppendLine($"{new string(' ', _indentLevel * _indentString.Length)}{text}");
        return this;
    }

    public IndentedStringBuilder AppendJoin(string separator, IEnumerable<string> values)
    {
        _builder.Append($"{new string(' ', _indentLevel * _indentString.Length)}{string.Join(separator, values)}");
        return this;
    }

    public IndentedStringBuilder Append(string text)
    {
        _builder.Append($"{new string(' ', _indentLevel * _indentString.Length)}{text}");
        return this;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}

