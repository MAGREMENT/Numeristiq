using System.Collections.Generic;

namespace Model.Helpers.Descriptions;

public class MultipleLineDescription : IDescription
{
    private List<IDescriptionLine> _lines;

    public IEnumerable<IDescriptionLine> EnumerateLines()
    {
        return _lines;
    }

    public MultipleLineDescription Add(IDescriptionLine line)
    {
        _lines.Add(line);
        return this;
    }
}