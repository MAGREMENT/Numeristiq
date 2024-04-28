using System;
using System.ComponentModel;
using System.Globalization;

namespace DesktopApplication.View.Utility;

[TypeConverter(typeof(DependantThicknessRangeTypeConverter))]
public readonly struct DependantThicknessRange
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int Floor { get; init; }
    public int Roof { get; init; }

    public int GetValueFor(double n)
    {
        if (n >= Roof) return Maximum;
        if (n <= Floor) return Minimum;
        return Minimum + (int)Math.Round((n - Floor) / (Roof - Floor) * (Maximum - Minimum));
    }
}

public class DependantThicknessRangeTypeConverter : TypeConverter
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string s) return null;

        var numbers = new int[4];
        int cursor = 0;
        int buffer = 0;
        foreach (var c in s)
        {
            if (char.IsDigit(c))
            {
                buffer *= 10;
                buffer += c - '0';
            }
            else
            {
                if (buffer == 0) continue;
                
                if (cursor > numbers.Length) return null;

                numbers[cursor++] = buffer;
                buffer = 0;
            }
        }

        if (buffer != 0)
        {
            if (cursor > numbers.Length) return null;

            numbers[cursor] = buffer;
        }

        return new DependantThicknessRange
        {
            Minimum = numbers[0],
            Maximum = numbers[1],
            Floor = numbers[2],
            Roof = numbers[3]
        };
    }

    public override object? ConvertTo(ITypeDescriptorContext? context,
        CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string)) return null;
        
        if (value is not DependantThicknessRange tr) return null;

        return $"{tr.Maximum}, {tr.Minimum}, {tr.Floor}, {tr.Roof}";
    }
}