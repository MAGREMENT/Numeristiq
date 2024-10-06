using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Model.XML;

public static class XMLParser
{
    public static IEnumerable<IXMLElement> Parse(string file)
    {
        using var reader = new StreamReader(file, new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.None
        });
        StringBuilder builder = new();
        List<Tag> tagQueue = new();
        
        while (!reader.EndOfStream)
        {
            var c = (char)reader.Read();
            switch (c)
            {
                case ' ' :
                case '\n' :
                case '\r' :
                case '\t':
                    break;
                case '<' :
                    if (reader.EndOfStream) throw new Exception("Tag not closed");

                    var next = (char)reader.Read();
                    if (next == '/')
                    {
                        var name = ReadClosingTag(reader, builder);
                        if (tagQueue.Count == 0) throw new Exception("No tag to close");
                            
                        if (!tagQueue[^1].Name.Equals(name)) throw new Exception("Wrong enclosing tag");

                        if (tagQueue.Count == 1) yield return tagQueue[0];
                        tagQueue.RemoveAt(tagQueue.Count - 1);
                    }
                    else
                    {
                        var (tag, isAutoClosed) = ReadOpeningTag(reader, builder, next);
                        if (isAutoClosed)
                        {
                            if (tagQueue.Count == 0) yield return tag;
                            else tagQueue[^1].AddToContent(tag);
                        }
                        else
                        {
                            if (tagQueue.Count != 0) tagQueue[^1].AddToContent(tag);
                            tagQueue.Add(tag);
                        }
                    }

                    break;
                default :
                    var s = ReadText(reader, builder, c);
                    if (tagQueue.Count == 0) yield return new Text(s);
                    else tagQueue[^1].AddToContent(s);
                    
                    break;
            }
        }
    }

    private static string ReadText(StreamReader reader, StringBuilder builder, char start)
    {
        builder.Clear();
        builder.Append(start);

        while (!reader.EndOfStream)
        {
            var peeked = (char)reader.Peek();
            if (peeked == '<') break;

            builder.Append((char)reader.Read());
        }

        return builder.ToString().TrimEnd('\n', '\r', ' ', '\t');
    }

    private static (Tag, bool) ReadOpeningTag(StreamReader reader, StringBuilder builder, char start)
    {
        if (!IsAlphabetical(start)) throw new Exception("Cannot start tag with : " + start);
        
        builder.Clear();
        builder.Append(start);
        Tag? tag = null;
        
        while (!reader.EndOfStream)
        {
            var c = (char)reader.Read();
            switch (c)
            {
                case '/' :
                    if (reader.EndOfStream) throw new Exception("Expected '>' after '/'");
                    
                    var end = (char)reader.Read();
                    if(end != '>') throw new Exception("Expected '>' after '/'");

                    if (tag is not null) return (tag, true);
                    
                    if (builder.Length == 0) throw new Exception("Empty tag");

                    return (new Tag(builder.ToString()), true);
                case '>' :
                    if (tag is not null) return (tag, false);

                    if (builder.Length == 0) throw new Exception("Empty tag");

                    return (new Tag(builder.ToString()), false);
                case ' ' :
                    if (!SkipSpacing(reader)) throw new Exception("Unfinished Tag");
                    
                    tag ??= new Tag(builder.ToString());
                    break;
                default:
                    if (tag is not null)
                    {
                        var (name, value) = ReadAttribute(reader, builder);
                        tag.AddAttribute(name, value);
                        break;
                    }
                    
                    if (!IsAlphabetical(c))
                        throw new Exception("Unexpected character in tag : " + c);

                    builder.Append(c);
                    break;
            }
        }

        throw new Exception("Tag not closed");
    }

    private static bool SkipSpacing(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            if ((char)reader.Peek() != ' ') return true; //TODO other than spaces ?
            reader.Read();
        }

        return false;
    }

    private static (string, string) ReadAttribute(StreamReader reader, StringBuilder builder)
    {
        builder.Clear();
        string? name = null;
        
        while (!reader.EndOfStream)
        {
            var c = (char)reader.Read();
            switch (c)
            {
                case '=':
                    if (name is not null) throw new Exception("Attribute name already defined, equal sign not valid");

                    if (builder.Length == 0) throw new Exception("Empty attribute name");

                    if (reader.EndOfStream) throw new Exception("Expected attribute value");
                    
                    var end = reader.Read();
                    if (end != '"') throw new Exception("Expected '\"' after '='");

                    name = builder.ToString();
                    builder.Clear();
                    break;
                case '"' :
                    if (name is null) throw new Exception("'\"' before attribute name definition");

                    if (builder.Length == 0) throw new Exception("Empty attribute value");

                    return (name, builder.ToString());
                default :
                    if (!IsAlphabetical(c))
                        throw new Exception("Unexpected character in attribute : " + c);

                    builder.Append(c);
                    break;
            }
        }

        throw new Exception("Unfinished attribute");
    }

    private static bool IsAlphabetical(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '-';
    }

    private static string ReadClosingTag(StreamReader reader, StringBuilder builder)
    {
        builder.Clear();
        
        while (!reader.EndOfStream)
        {
            var c = (char)reader.Read();
            switch (c)
            {
                case '>' :
                    if (builder.Length == 0) throw new Exception("Empty closing tag");

                    return builder.ToString();
                default:
                    if (!IsAlphabetical(c))
                        throw new Exception("Unexpected character in closing tag : " + c);

                    builder.Append(c);
                    break;
            }
        }
        
        throw new Exception("Tag not closed");
    }
}

public interface IXMLElement
{
    bool IsTag { get; }
    string GetStringValue();
    Tag GetTagValue();
}