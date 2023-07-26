namespace Model.StrategiesUtil.LoopFinder.Types;

public interface ILoopType<T> where T : ILoopElement
{
    void Apply(LoopFinder<T> manager);
}