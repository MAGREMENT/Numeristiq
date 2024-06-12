using System.Collections.Generic;

namespace Model.Core.Descriptions;

public class FullTextDescription : IDescription
{
    private readonly TextDescriptionLine _line;
    
    public FullTextDescription(string text)
    {
        _line = new TextDescriptionLine(text);
    }
    
    public IEnumerable<IDescriptionLine> EnumerateLines()
    {
        yield return _line;
    }
}