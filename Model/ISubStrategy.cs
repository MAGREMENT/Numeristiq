namespace Model;

public interface ISubStrategy
{
    bool ApplyOnce(ISolver solver);
}