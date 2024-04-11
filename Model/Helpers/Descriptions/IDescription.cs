using System.Collections.Generic;

namespace Model.Helpers.Descriptions;

public interface IDescription
{
    public IEnumerable<IDescriptionLine> EnumerateLines();
}