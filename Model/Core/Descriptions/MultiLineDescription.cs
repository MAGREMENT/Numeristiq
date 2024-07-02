using System.Collections.Generic;

namespace Model.Core.Descriptions;

public class MultiLineDescription : IDescription
{
    private readonly List<IDescriptionLine> _lines = new();

    public IEnumerable<IDescriptionLine> EnumerateLines()
    {
        return _lines;
    }

    public MultiLineDescription Add(IDescriptionLine line)
    {
        _lines.Add(line);
        return this;
    }
}