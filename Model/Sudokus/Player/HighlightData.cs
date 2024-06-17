using System;
using System.Collections.Generic;

namespace Model.Sudokus.Player;

public readonly struct HighlightData
{
    private const int PossibilityColorBitSize = 3;
    private const uint PossibilityColorBitMask = 0x7;
    
    private readonly ushort _cellColors;
    private readonly uint _bottomPossibilitiesColor;
    private readonly uint _middlePossibilitiesColor;
    private readonly uint _topPossibilitiesColor;

    public int Count => System.Numerics.BitOperations.PopCount(_cellColors) +
                        System.Numerics.BitOperations.PopCount(_bottomPossibilitiesColor) +
                        System.Numerics.BitOperations.PopCount(_middlePossibilitiesColor) +
                        System.Numerics.BitOperations.PopCount(_topPossibilitiesColor);

    public HighlightData()
    {
        _cellColors = 0;
        _bottomPossibilitiesColor = 0;
        _middlePossibilitiesColor = 0;
        _topPossibilitiesColor = 0;
    }

    private HighlightData(ushort cellColors, uint bottomPossibilitiesColor, uint middlePossibilitiesColor,
        uint topPossibilitiesColor)
    {
        _cellColors = cellColors;
        _bottomPossibilitiesColor = bottomPossibilitiesColor;
        _middlePossibilitiesColor = middlePossibilitiesColor;
        _topPossibilitiesColor = topPossibilitiesColor;
    }

    public HighlightData ApplyColorToCell(HighlightColor color)
    {
        return ((_cellColors >> (int)color) & 1) > 0 
            ? new HighlightData((ushort)(_cellColors | (1 << (int)color)), _bottomPossibilitiesColor,
                _middlePossibilitiesColor, _topPossibilitiesColor) 
            : new HighlightData((ushort)(_cellColors & ~(1 << (int)color)), _bottomPossibilitiesColor,
                _middlePossibilitiesColor, _topPossibilitiesColor);
    }

    public HighlightData ApplyColorToPossibility(int possibility, HighlightColor color, PossibilitiesLocation location)
    {
        return location switch
        {
            PossibilitiesLocation.Bottom => new HighlightData(_cellColors, ApplyColorToPossibility(possibility, color,
                _bottomPossibilitiesColor), _middlePossibilitiesColor, _topPossibilitiesColor),
            PossibilitiesLocation.Middle => new HighlightData(_cellColors, _bottomPossibilitiesColor,
                ApplyColorToPossibility(possibility, color, _middlePossibilitiesColor), _topPossibilitiesColor),
            PossibilitiesLocation.Top => new HighlightData(_cellColors, _bottomPossibilitiesColor,
                _middlePossibilitiesColor, ApplyColorToPossibility(possibility, color, _topPossibilitiesColor)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private uint ApplyColorToPossibility(int possibility, HighlightColor color, uint colors)
    {
        var c = (uint)((int)color + 1);
        var shift = (possibility - 1) * PossibilityColorBitSize;
        var current = (colors >> shift) & PossibilityColorBitMask;
        var cleared = colors & ~(PossibilityColorBitMask << shift);
        return current == c ? cleared : colors | (c << shift);
    }
    
    public HighlightColor[] CellColorsToArray()
    {
        var result = new HighlightColor[System.Numerics.BitOperations.PopCount(_cellColors)];
        var cursor = 0;
        
        for(int i = 0; i < 7; i++)
        {
            if(((_cellColors >> i) & 1) > 0) result[cursor++] = (HighlightColor)i;
        }

        return result;
    }

    public IEnumerable<(int, HighlightColor)> CellPossibilitiesColorToEnumerable(PossibilitiesLocation location)
    {
        return location switch
        {
            PossibilitiesLocation.Bottom => CellPossibilitiesColorToEnumerable(_bottomPossibilitiesColor),
            PossibilitiesLocation.Middle => CellPossibilitiesColorToEnumerable(_middlePossibilitiesColor),
            PossibilitiesLocation.Top => CellPossibilitiesColorToEnumerable(_topPossibilitiesColor),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IEnumerable<(int, HighlightColor)> CellPossibilitiesColorToEnumerable(uint value)
    {
        for (int i = 0; i < 9; i++)
        {
            var c = (value >> (i * PossibilityColorBitSize)) & PossibilityColorBitMask;
            if (c != 0) yield return (i + 1, (HighlightColor)((int)c - 1));
        }
    }
}

public enum HighlightColor
{
    First, Second, Third, Fourth, Fifth, Sixth, Seventh
}