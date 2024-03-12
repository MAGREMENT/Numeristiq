namespace Model;

public interface IRepository<T> where T : class?
{ 
    public bool Initialize(bool createNewOnNoneExisting);
    public T? Download();
    public bool Upload(T DAO);
}

public enum CellColor
{
    Black, Gray, Red, Green, Blue
}

public enum StateShown
{
    Before, After
}

public enum RotationDirection
{
    ClockWise, CounterClockWise
}

public enum LinkOffsetSidePriority
{
    Any, Left, Right
}