﻿using System;
using System.Collections.Generic;
using System.Text;
using Model.Utility.Collections;

namespace Model.XML;

public class Tag : IXMLElement, ITagContent
{
    private Dictionary<string, string>? _attributes;
    
    public string Name { get; }
    public ITagContent? Content { get; private set; }
    
    public Tag(string name)
    {
        Name = name;
    }

    public void AddAttribute(string name, string value)
    {
        _attributes ??= new Dictionary<string, string>();
        _attributes.Add(name, value);
    }

    public string GetValue(string name)
    {
        if (_attributes is null) return string.Empty;
        return _attributes.TryGetValue(name, out var v) ? v : string.Empty;
    }

    public bool IsTag => true;

    public string GetStringValue()
    {
        return Content is null ? string.Empty : Content.GetStringValue();
    }

    public Tag GetTagValue() => this;

    public void SetContent(ITagContent content) => Content = content;
    public void SetContent(string content) => Content = new Text(content);

    public void AddToContent(ITagContent content)
    {
        if (Content is null)
        {
            Content = content;
            return;
        }

        var collection = new TagContentCollection { Content, content };
        Content = collection;
    }

    public void AddToContent(string content) => AddToContent(new Text(content));

    public override int GetHashCode()
    {
        var hash = Content is null ? Name.GetHashCode() : HashCode.Combine(Name.GetHashCode(), Content.GetHashCode());

        if (_attributes is not null)
        {
            foreach (var entry in _attributes)
            {
                HashCode.Combine(hash, entry.Key, entry.Value);
            }
        }

        return hash;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Tag t || !t.Name.Equals(Name)) return false;

        if (Content is null)
        {
            if (t.Content is not null) return false;
        }
        else if (!Content.Equals(t.Content)) return false;

        if (_attributes is null)
        {
            if (t._attributes is not null) return false;
        }
        else
        {
            if (t._attributes is null || t._attributes.Count != _attributes.Count) return false;

            foreach (var entry in t._attributes)
            {
                if (!_attributes.TryGetValue(entry.Key, out var v) || !v.Equals(entry.Value)) return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder("<");
        builder.Append(Name);

        if (_attributes is not null)
        {
            foreach (var attribute in _attributes)
            {
                builder.Append($" {attribute.Key}=\"{attribute.Value}\"");
            }

            builder.Append(' ');
        }

        if (Content is null)
        {
            builder.Append("/>");
            return builder.ToString();
        }

        builder.Append(">\n");
        builder.Append(Content);
        builder.Append($"</{Name}>");
        return builder.ToString();
    }
}

public class TagContentCollection : List<ITagContent>, ITagContent
{
    public string GetStringValue()
    {
        var builder = new StringBuilder();
        foreach (var tag in this)
        {
            builder.Append(tag.GetStringValue());
        }

        return builder.ToString();
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var e in this)
        {
            HashCode.Combine(hash, e);
        }

        return hash;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TagContentCollection collection || collection.Count != Count) return false;

        for (int i = 0; i < collection.Count; i++)
        {
            if (!collection[i].Equals(this[i])) return false;
        }

        return true;
    }

    public override string ToString()
    {
        return this.ToStringSequence("\n");
    }
}

public class Text : IXMLElement, ITagContent
{
    private readonly string _value;

    public Text(string value)
    {
        _value = value;
    }

    public bool IsTag => false;

    public string GetStringValue()
    {
        return _value;
    }

    public Tag GetTagValue()
    {
        throw new Exception("Not a tag");
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Text t && t._value == _value;
    }

    public override string ToString()
    {
        return _value;
    }
}

public interface ITagContent
{
    string GetStringValue();
}