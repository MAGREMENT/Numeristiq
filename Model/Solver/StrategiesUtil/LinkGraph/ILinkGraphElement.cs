namespace Model.Solver.StrategiesUtil.LinkGraph;

public interface ILinkGraphElement : ILoopElement
{
    public Cell[] EveryCell();
}