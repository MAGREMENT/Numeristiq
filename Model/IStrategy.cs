namespace Model;

public interface IStrategy
{
    bool ApplyOnce(ISolver solver);
    bool ApplyUntilProgress(ISolver solver);
}