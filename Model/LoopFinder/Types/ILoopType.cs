using Model.LoopFinder;

namespace LoopFinder.Strategies;

public interface ILoopType<T> where T : notnull
{
    void Apply(LoopFinder<T> manager);
}