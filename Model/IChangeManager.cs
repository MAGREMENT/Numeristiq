using System.Collections.Generic;
using Model.Logs;
using Model.Possibilities;

namespace Model;

public interface IChangeManager
{
    public IPossibilities[,] Possibilities { get; }
    public bool LogsManaged { get; }
    public bool AddDefinitive(int number, int row, int col);
    public bool RemovePossibility(int possibility, int row, int col);
    public void PushLog(IEnumerable<LogChange> changes, IEnumerable<LogCause> causes, string explanation, IStrategy strategy);
}