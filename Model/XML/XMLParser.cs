﻿using System;
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
        int spaceCount = 0;
        
        while (!reader.EndOfStream)
        {
            var c = (char)reader.Read();
            switch (c)
            {
                case ' ' :
                    spaceCount++;
                    break;
                case '\n' :
                case '\r' :
                    spaceCount = 0;
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

                    spaceCount = 0;
                    break;
                default :
                    var s = ReadText(reader, builder, c, spaceCount);
                    if (tagQueue.Count == 0) yield return new Text(s);
                    else tagQueue[^1].AddToContent(s);

                    spaceCount = 0;
                    break;
            }
        }
    }

    private static string ReadText(StreamReader reader, StringBuilder builder, char start, int ignorable)
    {
        builder.Clear();
        builder.Append(start);

        var canIgnore = true;
        var streak = 0;
        while (!reader.EndOfStream)
        {
            var peeked = (char)reader.Peek();
            if (peeked == '<') break;

            var c = (char)reader.Read();
            switch (c)
            {
                case '\n' :
                case '\r' :
                    builder.Append(c);
                    canIgnore = true;
                    streak = 0;
                    break;
                case ' ' :
                    if (!canIgnore || streak >= ignorable) builder.Append(c);
                    else streak++;
                    break;
                default:
                    builder.Append(c);
                    canIgnore = false;
                    break;
            }
        }

        return builder.ToString().TrimEnd('\n', '\r', ' ');
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
                case '\r' :
                case '\n' :
                case ' ' :
                    if (!SkipSpacing(reader)) throw new Exception("Unfinished Tag");
                    
                    tag ??= new Tag(builder.ToString());
                    break;
                default:
                    if (tag is not null)
                    {
                        var (name, value) = ReadAttribute(reader, builder, c);
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
            var c = (char)reader.Peek();
            if (c is not ' ' and not '\n' and not '\r') return true;
            reader.Read();
        }

        return false;
    }

    private static (string, string) ReadAttribute(StreamReader reader, StringBuilder builder, char start)
    {
        builder.Clear();
        builder.Append(start);
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