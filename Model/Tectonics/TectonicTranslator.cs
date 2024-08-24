using System;
using System.Collections.Generic;
using System.Text;
using Model.Utility;

namespace Model.Tectonics;

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

        var split = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2) return new BlankTectonic();

        var separator = split[0].IndexOf('.');
        try
        {
            ITectonic result = new ArrayTectonic(int.Parse(split[0][..separator]),
                int.Parse(split[0][(separator + 1)..]));

            int row = 0, col = 0, buffer = 0;
            foreach (var c in split[1])
            {
                switch (c)
                {
                    case '.' :
                        if (buffer != 0) result[row, col] = buffer;
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
                        if (col / result.ColumnCount == 1)
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
            
            foreach (var list in zones.Values)
            {
                if(list.Count > IZone.MaxCount) return new BlankTectonic();
                    
                result.AddZoneUnchecked(list);
            }

            return result;
        }
        catch (Exception)
        {
            return new BlankTectonic();
        }
    }

    public static string TranslateRdFormat(IReadOnlyTectonic tectonic)
    {
        if (tectonic.RowCount == 0 || tectonic.ColumnCount == 0) return "";
        
        var builder = new StringBuilder($"{tectonic.RowCount}.{tectonic.ColumnCount}:");

        for (int row = 0; row < tectonic.RowCount; row++)
        {
            for (int col = 0; col < tectonic.ColumnCount; col++)
            {
                builder.Append(tectonic[row, col]);

                var current = new Cell(row, col);
                if (col < tectonic.ColumnCount - 1 && tectonic.IsFromSameZone(current, new Cell(row, col + 1)))
                    builder.Append('r');

                if (row < tectonic.RowCount - 1 && tectonic.IsFromSameZone(current, new Cell(row + 1, col)))
                    builder.Append('d');
            }
        }

        return builder.ToString();
    }

    public static ITectonic TranslateRdFormat(string s)
    {
        var split = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2) return new BlankTectonic();
        
        var separator = split[0].IndexOf('.');

        try
        {
            ITectonic result = new ArrayTectonic(int.Parse(split[0][..separator]),
                int.Parse(split[0][(separator + 1)..]));

            int row = 0, col = -1;
            foreach (var c in split[1])
            {
                switch (c)
                {
                    case 'r' :
                        if (col < result.ColumnCount - 1) result.MergeZones(new Cell(row, col), new Cell(row, col + 1));
                        break;
                    case 'd' :
                        if (row < result.RowCount - 1) result.MergeZones(new Cell(row, col), new Cell(row + 1, col));
                        break;
                    default :
                        col++;
                        if (col / result.ColumnCount == 1)
                        {
                            col = 0;
                            row++;
                        }

                        result[row, col] = c - '0';
                        break;
                }
            }

            return result;
        }
        catch (Exception)
        {
            return new BlankTectonic();
        }
    }

    public static TectonicStringFormat GuessFormat(string s)
    {
        return s.Contains(';') ? TectonicStringFormat.Code : TectonicStringFormat.Rd;
    }
}

public enum TectonicStringFormat
{
    None, Code, Rd
}