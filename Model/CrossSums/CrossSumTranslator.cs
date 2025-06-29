using System.Text;

namespace Model.CrossSums;

public static class CrossSumTranslator
{
    public static string Translate(IReadOnlyCrossSum cs)
    {
        var builder = new StringBuilder();

        for (int c = 0; c < cs.ColumnCount; c++)
        {
            if (c > 0) builder.Append('.');
            builder.Append(cs.GetExpectedForColumn(c));
        }

        builder.Append("::");
        
        for (int r = 0; r < cs.RowCount; r++)
        {
            if (r > 0) builder.Append('.');
            builder.Append(cs.GetExpectedForRow(r));
        }
        
        builder.Append("::");

        for (int r = 0; r < cs.RowCount; r++)
        {
            for (int c = 0; c < cs.ColumnCount; c++)
            {
                if (cs.IsChosen(r, c)) builder.Append('o');
                builder.Append(cs[r, c]);
            }
        }
        
        return builder.ToString();
    }

    public static CrossSum Translate(string s)
    {
        var split = s.Split("::");
        if (split.Length != 3) return new CrossSum(0, 0);

        var cols = split[0].Split('.');
        var rows = split[1].Split('.');

        var cs = new CrossSum(rows.Length, cols.Length);

        for (int c = 0; c < cols.Length; c++)
        {
            if (!int.TryParse(cols[c], out var n)) return new CrossSum(0, 0);
            cs.AddToExpectedForColumn(c, n);
        }
        
        for (int r = 0; r < rows.Length; r++)
        {
            if (!int.TryParse(rows[r], out var n)) return new CrossSum(0, 0);
            cs.AddToExpectedForRow(r, n);
        }

        var values = split[2];
        int currRow = 0, currCol = 0, index = 0;

        while (index < values.Length && currRow < rows.Length)
        {
            var c = values[index];
            if (c == 'o') cs.Choose(currRow, currCol);
            else
            {
                var n = c - '0';
                if(n > 9 || n < 1) return new CrossSum(0, 0);

                cs[currRow, currCol] = n;
            }
            
            index++;
            currCol++;
            if (currCol >= cols.Length)
            {
                currCol = 0;
                currRow++;
            }
        }

        return cs;
    }
}