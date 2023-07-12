namespace Model;

public interface ISubStrategy
{
    //TODO : remove bool
    bool ApplyOnce(ISolver solver);
}