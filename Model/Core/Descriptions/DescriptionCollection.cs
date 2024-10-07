using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Descriptions;

public class DescriptionCollection<TDisplayer> : IDescription<TDisplayer> where TDisplayer : IDescriptionDisplayer
{
    private readonly List<IDescription<TDisplayer>> _list = new();
    
    public DescriptionCollection<TDisplayer> Add(IDescription<TDisplayer> description)
    {
        _list.Add(description);
        return this;
    }

    public DescriptionCollection<TDisplayer> Add(string s)
    {
        _list.Add(new TextDescription<TDisplayer>(s));
        return this;
    }

    public void Display(TDisplayer displayer)
    {
        foreach (var d in _list) d.Display(displayer);
    }

    public override int GetHashCode() => _list.GetSequenceHashCode();

    public override bool Equals(object? obj)
    {
        return obj is DescriptionCollection<TDisplayer> d && d._list.SequenceEqual(_list);
    }
}