using System;
using Model.Core.Descriptions;
using Model.XML;

namespace Model.Sudokus.Solver.Descriptions;

public static class SudokuDescriptionParser
{
    public static IDescription<ISudokuDescriptionDisplayer> FromXML(string path)
    {
        var parsed = XMLParser.Parse(path);
        var result = new DescriptionCollection<ISudokuDescriptionDisplayer>();
        foreach (var element in parsed)
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

    private static SudokuCropping CastCropping(string s)
    {
        Span<int> span = stackalloc int[4];
        int cursor = 0;
        int buffer = 0;
        foreach (var c in s)
        {
            if (c is ',' or ';' or ' ')
            {
                span[cursor++] = buffer;
                if (cursor == 4) break;
                buffer = 0;
            }

            buffer *= 10;
            buffer += c - '0';
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