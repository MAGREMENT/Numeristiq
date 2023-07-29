namespace Model.StrategiesUtil.LoopFinder.Types;

public interface ILoopType<T> where T : ILoopElement, ILinkGraphElement
{
    void Apply(LoopFinder<T> manager);
}