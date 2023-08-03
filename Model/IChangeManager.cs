using System.Collections.Generic;
using Model.Logs;

namespace Model;

public interface IChangeManager
{
    public bool AddDefinitive(int number, int row, int col);
    public bool RemovePossibility(int possibility, int row, int col);

    public void PushLog(IEnumerable<LogChange> changes, IEnumerable<LogCause> causes);
}