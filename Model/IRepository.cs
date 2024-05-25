namespace Model;

public interface IRepository<T> where T : class?
{ 
    public T? Download();
    public bool Upload(T DAO);
}

