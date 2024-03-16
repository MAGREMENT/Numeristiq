using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

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

        var finalZones = new IZone[zones.Count];
        int cursor = 0;
        foreach (var list in zones.Values)
        {
            finalZones[cursor++] = new MultiZone(list.ToArray(), colCount);
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

        CellsAssociations associatedCells = new(rowCount, colCount);
        Dictionary<Cell, int> numbers = new();
        int row = 0;
        int col = -1;
        
        foreach (var c in split[1])
        {
            var current = new Cell(row, col);
            
            switch (c)
            {
                case 'r' : 
                    if(col >= colCount - 1) continue;
                    associatedCells.Merge(current, new Cell(row, col + 1));
                    
                    break;
                case 'd' :
                    if (row >= rowCount - 1) continue;
                    associatedCells.Merge(current, new Cell(row + 1, col));

                    break;
                default:
                    if (!char.IsDigit(c)) return new BlankTectonic();

                    var n = c - '0';
                    
                    col++;
                    if (col == colCount)
                    {
                        col = 0;
                        row++;
                    }
                    numbers.Add(new Cell(row, col), n);
                    
                    break;
            }
        }

        List<IZone> zones = new();
        List<Cell> buffer = new();
        for (row = 0; row < rowCount; row++)
        {
            for (col = 0; col < colCount; col++)
            {
                var current = new Cell(row, col);
                if (!associatedCells.IsCreatedAt(row, col))
                {
                    zones.Add(new MultiZone(current, colCount));
                }
                else
                {
                    var set = associatedCells.SetAt(row, col);
                    if(Contains(zones, set, colCount)) continue;
                    
                    foreach (var n in set)
                    {
                        buffer.Add(new Cell(n / colCount, n % colCount));
                    }

                    zones.Add(new MultiZone(buffer.ToArray(), colCount));
                    buffer.Clear();
                }
            }
        }

        var result = new ArrayTectonic(rowCount, colCount, zones);

        foreach (var entry in numbers)
        {
            result.Set(entry.Value, entry.Key.Row, entry.Key.Column);
        }

        return result;
    }

    private static bool Contains(IReadOnlyList<IZone> zones, InfiniteBitSet bitSet, int colCount)
    {
        foreach (var zone in zones)
        {
            if (zone.Count != bitSet.Count) continue;

            var yes = true;
            foreach (var cell in zone)
            {
                if (!bitSet.IsSet(cell.Row * colCount + cell.Column))
                {
                    yes = false;
                    break;
                }
            }

            if(yes) return true;
        }
        
        return false;
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