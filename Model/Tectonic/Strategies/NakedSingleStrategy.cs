namespace Model.Tectonic.Strategies;

public class NakedSingleStrategy : AbstractStrategy
{
    public override void Apply(ISolvable solvable)
    {
        foreach (var cell in solvable.Tectonic.EachCell())
        {
            var candidates = solvable.GetCandidates(cell);
            if (candidates.Count == 1)
            {
                //Make definitive
            }
        }
    }
}