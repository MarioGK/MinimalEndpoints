using System;
using System.Text;
using Cysharp.Text;

namespace MinimalEndpoints.Helpers;

internal class IndentedStringBuilder
{
    private readonly StringBuilder _sb = new();
    private int _indent;
    private const int IndentSize = 4;

    public void IncrementIndent() => _indent++;

    public void DecrementIndent()
    {
        if (_indent > 0) _indent--;
    }

    public IDisposable Indent()
    {
        IncrementIndent();
        return new IndentDisposable(this);
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    public void AppendLine(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _sb.Append(' ', _indent * IndentSize);
            _sb.AppendLine(text);
        }
        else
        {
            _sb.AppendLine();
        }
    }

    public void AppendLine(string format, params object[] args)
    {
        _sb.Append(' ', _indent * IndentSize);
        _sb.AppendLine(ZString.Format(format, args));
    }

    public override string ToString() => _sb.ToString();

    private class IndentDisposable : IDisposable
    {
        private readonly IndentedStringBuilder _isb;
        public IndentDisposable(IndentedStringBuilder isb) => _isb = isb;
        public void Dispose() => _isb.DecrementIndent();
    }
}
