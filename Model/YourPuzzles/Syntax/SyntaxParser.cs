using System;
using System.Collections.Generic;
using Model.Utility.Collections;

namespace Model.YourPuzzles.Syntax;

public class SyntaxParser
{
    private readonly Dictionary<string, ISyntaxParsable> _simpleStrings = new();
    private readonly List<ISyntaxParsable> _others = new();
    
    public SyntaxParser(IEnumerable<ISyntaxParsable> parsables)
    {
        foreach (var p in parsables)
        {
            if (p.IsSimpleString is not null) _simpleStrings.Add(p.IsSimpleString, p);
            else _others.Add(p);
        }
    }

    public IReadOnlyList<BinaryTreeNode<ISyntaxElement>> ParseText(string s)
    {
        List<BinaryTreeNode<ISyntaxElement>> result = new();
        int start = 0;
        for (int current = 0; current < s.Length; current++)
        {
            if (current != s.Length - 1 && s[current] != '\n') continue;
            
            if (current != start)
            {
                var e = TryParseLine(s.AsSpan(start, current - start), out var b);
                if(e == ParserError.None && b is not null) result.Add(b);
            }

            start = current + 1;
        }

        return result;
    }

    public ParserError? TryParseLine(ReadOnlySpan<char> s, out BinaryTreeNode<ISyntaxElement>? result, int line = 0)
    {
        result = null;
        BinaryTreeNode<ISyntaxElement>? node = null;
        int start = 0;
        for (int current = 0; current <= s.Length; current++)
        {
            if (current != s.Length && s[current] != ' ') continue;
            
            if (current != start)
            {
                var span = s.Slice(start, current - start).ToString();
                ISyntaxElement? el = null;
                if (_simpleStrings.TryGetValue(span, out var parsable)) el = parsable.Parse(span);
                else
                {
                    foreach (var other in _others)
                    {
                        el = other.Parse(span);
                        if (el is not null) break;
                    }
                }

                if (el is not null)
                {
                    if (node is null)
                    {
                        node = new BinaryTreeNode<ISyntaxElement>(el);
                        result = node;
                    }
                    else switch (el.Type)
                    {
                        case SyntaxElementType.Operator:
                            if (node.Value.Type == SyntaxElementType.Value)
                            {
                                node.SetLeft(node.Value);
                                node.Value = el;
                                break;
                            }
                            
                            if (node.Left is null) return new ParserError(line, start, ParseErrorType.OperatorStart);
                            if (node.Right is null) return new ParserError(line, start, ParseErrorType.DoubleOperator);

                            if (el.Priority < node.Value.Priority)
                            {
                                var buffer = result;
                                result = new BinaryTreeNode<ISyntaxElement>(el);
                                result.Left = buffer;
                                node = result;
                            }
                            else if (node.Right.Value.Type == SyntaxElementType.Operator)
                                return new ParserError(0, start, ParseErrorType.DoubleOperator);
                            else
                            {
                                var buffer = new BinaryTreeNode<ISyntaxElement>(el);
                                buffer.Left = node.Right;
                                node.Right = buffer;
                                node = node.Right;
                            }
                            
                            break;
                        case SyntaxElementType.Value:
                            if(node.Value.Type == SyntaxElementType.Value) 
                                return new ParserError(line, start, ParseErrorType.DoubleValue);
                            
                            if (node.Left is null) node.SetLeft(el);
                            else if (node.Right is null) node.SetRight(el);
                            else return new ParserError(line, start, ParseErrorType.DoubleValue);
                            
                            break;
                    }
                }
            }

            start = current + 1;
        }
        
        return ParserError.None;
    }
}

public enum ParseErrorType
{
    DoubleOperator, DoubleValue, OperatorStart
}

public class ParserError
{
    public static readonly ParserError? None = null;

    public ParserError(int line, int column, ParseErrorType error)
    {
        Line = line;
        Column = column;
        Error = error;
    }

    public int Line { get; }
    public int Column { get; }
    public ParseErrorType Error { get; }
}