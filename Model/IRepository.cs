namespace Model;

public interface IRepository<T> where T : class?
{ 
    public bool Initialize(bool createNewOnNoneExisting);
    public T? Download();
    public bool Upload(T DAO);
}

