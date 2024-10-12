using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Model.Core;
using Model.Core.Descriptions;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility;
using Model.XML;

namespace Model.Sudokus.Solver.Descriptions;

public class SudokuDescriptionParser : DescriptionParser<ISudokuDescriptionDisplayer>
{
    public SudokuDescriptionParser(string directory, bool searchParentDirectories, bool createIfNotFound) 
        : base(directory, searchParentDirectories, createIfNotFound) { }

    protected override IDescription<ISudokuDescriptionDisplayer> ParseXml(string path)
    {
        return FromXML(path);
    }
    
    protected override IDescription<ISudokuDescriptionDisplayer> ParseTxt(string path)
    {
        return FromTxt(path);
    }

    public static IDescription<ISudokuDescriptionDisplayer> FromTxt(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8);
        return new TextDescription<ISudokuDescriptionDisplayer>(reader.ReadToEnd());
    }
    
    public static IDescription<ISudokuDescriptionDisplayer> FromXML(string path)
    {
        var parsed = XMLParser.Parse(path).ToArray();
        var result = new DescriptionCollection<ISudokuDescriptionDisplayer>();
        if (parsed.Length == 0) return result;

        IEnumerable<IXMLElement> enumerable = parsed;
        if (parsed.Length == 1 && parsed[0].IsTag)
        {
            var root = parsed[0].GetTagValue();
            if (root.Name == "description") enumerable = root;
        }
        
        foreach (var element in enumerable)
        {
            if (element.IsTag)
            {
                var tag = element.GetTagValue();
                switch (tag.Name)
                {
                    case "p":
                        result.Add(new TextDescription<ISudokuDescriptionDisplayer>(tag.GetStringValue()));
                        break;
                    case "step":
                        if (!tag.TryGetAttributeValue("state", out var state))
                            throw new Exception("step tag need state attribute");

                        result.Add(new SudokuStepDescription(
                            tag.GetStringValue(),
                            state,
                            tag.GetAttributeValue("cropping", SudokuCropping.Default(), CastCropping),
                            tag.GetAttributeValue("highlight"),
                            tag.GetAttributeValue("disposition", TextDisposition.Left, CastDisposition)));
                        break;
                }
            }
            else
            {
                result.Add(new TextDescription<ISudokuDescriptionDisplayer>(element.GetStringValue()));
            }
        }

        return result;
    }

    public static string ToXml(IStep<ISudokuHighlighter, ISudokuSolvingState> step, StepState ss)
    {
        var state = ss == StepState.From ? step.From : step.To;
        return $"<step state=\"{SudokuTranslator.TranslateBase32Format(state, DefaultBase32Alphabet.Instance)}\" \n" +
               $"highlight=\"{step.HighlightCollection.TryGetInstructionsAsString()}\" \n" +
               $"cropping=\"0 0 8 8\" disposition=\"left\"></step>";
    }

    private static SudokuCropping CastCropping(string s)
    {
        Span<int> span = stackalloc int[4];
        int cursor = 0;
        int buffer = 0;
        var wasEscape = true;
        foreach (var c in s)
        {
            if (c is ',' or ';' or ' ')
            {
                if (wasEscape) continue;
                
                span[cursor++] = buffer;
                if (cursor == 4) break;
                buffer = 0;

                wasEscape = true;
                continue;
            }

            buffer *= 10;
            buffer += c - '0';
            wasEscape = false;
        }

        if (buffer != 0 && cursor < 4)
        {
            span[cursor] = buffer;
        }

        return new SudokuCropping(span[0], span[1], span[2], span[3]);
    }

    private static TextDisposition CastDisposition(string s)
    {
        return s switch
        {
            "right" => TextDisposition.Right,
            _ => TextDisposition.Left
        };
    }
}