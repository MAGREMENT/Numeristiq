using System.Collections.Generic;

namespace Model.Core.Descriptions;

public interface IDescription
{
    public IEnumerable<IDescriptionLine> EnumerateLines();
}