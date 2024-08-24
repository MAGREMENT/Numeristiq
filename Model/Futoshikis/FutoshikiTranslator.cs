using System;
using Model.Utility;

namespace Model.Futoshikis;

public static class FutoshikiTranslator
{
    public static Futoshiki TranslateLineFormat(string s)
    {
        var split = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 3) return new Futoshiki();

        try
        {
            var result = new Futoshiki(int.Parse(split[0]));

            int cursor = 0;
            bool isCounting = false;
            string buffer = "";
            foreach (var c in split[1])
            {
                switch (c)
                {
                    case 's' when isCounting:
                    {
                        cursor += int.Parse(buffer);
                        buffer = "";
                        isCounting = false;
                        break;
                    }
                    case 's':
                        isCounting = true;
                        break;
                    case ' ': case '.' :
                        result[cursor / result.Length, cursor % result.Length] = 0;
                        cursor++;
                        break;
                    default:
                    {
                        if (isCounting) buffer += c;
                        else
                        {
                            result[cursor / result.Length, cursor % result.Length] = int.Parse(c.ToString());
                            cursor++;
                        }

                        break;
                    }
                }
            }
            
            if(split[2].Length == 0) return result;

            foreach (var constraint in split[2].Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if(constraint.Length != 5) continue;

                var c1 = new Cell(constraint[0] - '0', constraint[1] - '0');
                var c2 = new Cell(constraint[3] - '0', constraint[4] - '0');
                result.AddConstraint(constraint[2] switch
                {
                    '>' => new BiggerThanConstraint(c1, c2),
                    '<' => new SmallerThanConstraint(c1, c2),
                    _ => throw new Exception()
                });
            }
            
            return result;
        }
        catch (Exception)
        {
            return new Futoshiki();
        }
    }
}