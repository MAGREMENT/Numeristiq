using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Core.Syntax;

public class SyntaxParser<T> where T : class, ISyntaxElement
{
    private readonly Dictionary<string, ISyntaxParsable<T>> _simpleStrings = new();
    private readonly List<ISyntaxParsable<T>> _others = new();
    
    public SyntaxParser(IEnumerable<ISyntaxParsable<T>> parsables)
    {
        foreach (var p in parsables)
        {
            if (p.IsSimpleString is not null) _simpleStrings.Add(p.IsSimpleString, p);
            else _others.Add(p);
        }
    }

    public ParserError? TryParseText(string s, out IReadOnlyList<ParsedLine> list)
    {
        var result = new List<ParsedLine>();
        var start = 0;
        var line = 0;
        for (int current = 0; current < s.Length; current++)
        {
            if (current != s.Length - 1 && s[current] != '\n') continue;
            
            if (current != start)
            {
                var e = TryParseLine(s.AsSpan(start, current - start), out var b, line);
                if (e != ParserError.None)
                {
                    list = Array.Empty<ParsedLine>();
                    return e;
                }
                
                if(b is not null) result.Add(new ParsedLine(line, b));
            }

            start = current + 1;
            line++;
        }

        list = result;
        return ParserError.None;
    }

    public ParserError? TryParseLine(ReadOnlySpan<char> s, out ISyntaxElement? result, int line = 0)
    {
        result = null;
        List<ISyntaxElement?> queue = new() { result };
        var index = 0;

        var start = 0;
        for (int current = 0; current <= s.Length; current++)
        {
            var closeBraces = false;
            
            var c = '\0';
            if (current < s.Length) c = s[current];
            switch (c)
            {
                case '(' :
                    queue.Add(null);
                    index++;
                    start = current + 1;
                    continue;
                case ')' :
                    closeBraces = true;
                    break;
                default:
                    if (current != s.Length && s[current] != ' ') continue;
                    break;
            }

            if (current != start)
            {
                var span = s.Slice(start, current - start).ToString();
                T? el = null;
                if (_simpleStrings.TryGetValue(span, out var parsable)) el = parsable.Parse(span);
                else
                {
                    foreach (var other in _others)
                    {
                        el = other.Parse(span);
                        if (el is not null) break;
                    }
                }

                if (el is null) return new ParserError(line, start, current, ParseErrorType.UnrecognizedElement);

                if(!TrySetNext(queue, index, el)) 
                    return new ParserError(line, start, current, ParseErrorType.Yikes);

                if (closeBraces)
                {
                    if (queue.Count <= 1) return new ParserError(line, start, current, ParseErrorType.TooManyClosingBraces);
                    
                    if(queue[index] is null) return new ParserError(line, start, current, ParseErrorType.EmptyBraces);

                    if (queue[index - 1] is not null)
                    {
                        if(!queue[index - 1]!.TryForceAppend(queue[index]!)) return new ParserError(line, start, current, ParseErrorType.Yikes);
                        queue.RemoveAt(queue.Count - 1);
                    }

                    index--;
                }
            }

            start = current + 1;
        }
        
        if(queue.Count != 1) return new ParserError(line, 0, s.Length, ParseErrorType.TooFewClosingBraces);

        result = queue[0];
        return ParserError.None;
    }

    private static bool TrySetNext(List<ISyntaxElement?> queue, int index, ISyntaxElement el)
    {
        if (queue[index] is null)
        {
            queue[index] = el;
            if (queue.Count > index + 1)
            {
                var result = queue[index]!.TryForceAppend(queue[index + 1]!);
                queue.RemoveAt(queue.Count - 1);
                return result;
            }
        }
        else if(!queue[index]!.TrySetNext(el))
        {
            var buffer = queue[index]!;
            queue[index] = el;
            if (!queue[index]!.TrySetNext(buffer)) return false;
        }

        return true;
    }
}

public readonly struct ParsedLine
{
    public readonly int Line;
    public readonly ISyntaxElement Value;

    public ParsedLine(int line, ISyntaxElement value)
    {
        Line = line;
        Value = value;
    }
}

public enum ParseErrorType
{
    UnrecognizedElement, TooManyClosingBraces, EmptyBraces, TooFewClosingBraces, Yikes
}

public class ParserError : ISyntaxError
{
    public static readonly ParserError? None = null;

    public ParserError(int line, int start, int end, ParseErrorType error)
    {
        Line = line;
        Start = start;
        End = end;
        Error = error;
    }

    public int Line { get; }
    public int Start { get; }
    public int End { get; }
    public ParseErrorType Error { get; }

    public (int, int, int) FindWhereToHighlight(string text) => (Line, Start, End);
    public string GetErrorMessage()
    {
        throw new NotImplementedException();
    }
}