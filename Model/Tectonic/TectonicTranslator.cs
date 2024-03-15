using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

/// <summary>
/// Code format : X.X:X.X;X.X;X.X;...
/// X = number
/// X.X: = dimensions (rows then columns)
/// X.X = number + zone (number can be omitted => ;.X;)
///
/// RD format : X.X:XrdXrXrXXrd...
/// X = number
/// X.X: = dimensions (rows then columns)
/// Xrd = number + r if same zone with number on the right + d if same zone with number down (r & d can be omitted)
/// </summary>
public static class TectonicTranslator
{
    public static ITectonic TranslateCodeFormat(string line)
    {
        Dictionary<int, List<Cell>> zones = new();
        Dictionary<Cell, int> numbers = new();
        int rowCount, colCount;

        var split = line.Split(':');
        if (split.Length != 2) return new BlankTectonic();

        int separator = split[0].IndexOf('.');
        try
        {
            rowCount = int.Parse(split[0][..separator]);
            colCount = int.Parse(split[0][(separator + 1)..]);

            int row = 0, col = 0, buffer = 0;
            foreach (var c in split[1])
            {
                switch (c)
                {
                    case '.' :
                        if (buffer != 0) numbers.Add(new Cell(row, col), buffer);
                        buffer = 0;
                        break;
                    case ';' :
                        if (!zones.TryGetValue(buffer, out var list))
                        {
                            list = new List<Cell>();
                            zones[buffer] = list;
                        }

                        list.Add(new Cell(row, col));
                        buffer = 0;
                        
                        col++;
                        if (col / colCount == 1)
                        {
                            col = 0;
                            row++;
                        }
                        break;
                    default:
                        buffer *= 10;
                        buffer += c - '0';
                        break;
                }
            }
        }
        catch (Exception)
        {
            return new BlankTectonic();
        }

        var finalZones = new Zone[zones.Count];
        int cursor = 0;
        foreach (var list in zones.Values)
        {
            finalZones[cursor++] = new Zone(list.ToArray(), colCount);
        }

        ITectonic result = new ArrayTectonic(rowCount, colCount, finalZones);
        foreach (var entry in numbers)
        {
            result[entry.Key] = entry.Value;
        }

        return result;
    }

    public static ITectonic TranslateRdFormat(string s)
    {
        var split = s.Split(':');
        if (split.Length != 2) return new BlankTectonic();
        
        int separator = split[0].IndexOf('.');

        if (!int.TryParse(split[0][..separator], out var rowCount)
            || !int.TryParse(split[0][(separator + 1)..], out var colCount)) return new BlankTectonic();

        return new BlankTectonic(); //TODO
    }
}