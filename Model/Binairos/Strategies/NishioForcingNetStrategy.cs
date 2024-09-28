using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Binairos.Strategies;

public class NishioForcingNetStrategy : Strategy<BinairoSolver>
{
    public const string OfficialName = "Nishio Forcing Net";
    
    public NishioForcingNetStrategy() : base(OfficialName, Difficulty.Extreme, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(BinairoSolver data)
    {
        data.ManagedGraph.Construct(BinairoConstructionRule.Instance);
        for (int r = 0; r < data.RowCount; r++)
        {
            for (int c = 0; c < data.ColumnCount; c++)
            {
                if(data[r, c] != 0) continue;
                
                for (int n = 1; n <= 2; n++)
                {
                    if (Search(data, new CellPossibility(r, c, n), data.ManagedGraph.Graph)) return;
                }
            }
        }
    }

    private bool Search(BinairoSolver data, CellPossibility first, IGraph<CellPossibility, LinkStrength> graph)
    {
        Dictionary<CellPossibility, CellPossibility> off = new();
        Dictionary<CellPossibility, CellPossibility> on = new();
        Queue<CellPossibility> queue = new();

        queue.Enqueue(first);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var offFriend in graph.Neighbors(current))
            {
                if (first == offFriend || on.ContainsKey(offFriend))
                {
                    data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(first));
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(off, on, offFriend));
                        if (StopOnFirstCommit) return true;
                    }
                    
                    continue;
                }
                
                if(!off.TryAdd(offFriend, current)) continue;

                if (off.ContainsKey(BinairoUtility.Opposite(offFriend)))
                {
                    data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(first));
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(off, on, offFriend));
                        if (StopOnFirstCommit) return true;
                    }
                    
                    continue;
                }

                foreach (var onFriend in graph.Neighbors(offFriend, LinkStrength.Strong))
                {
                    if (off.ContainsKey(onFriend))
                    {
                        data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(first));
                        if (data.ChangeBuffer.NeedCommit())
                        {
                            data.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(off, on, onFriend));
                            if (StopOnFirstCommit) return true;
                        }
                    
                        continue;
                    }
                    
                    if(!on.TryAdd(onFriend, current)) continue;

                    var opposite = BinairoUtility.Opposite(onFriend);
                    if (opposite == first || on.ContainsKey(opposite))
                    {
                        data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(first));
                        if (data.ChangeBuffer.NeedCommit())
                        {
                            data.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(off, on, onFriend));
                            if (StopOnFirstCommit) return true;
                        }
                    
                        continue;
                    }

                    var culprits = FindCulprits(onFriend, data, on, first);
                    if (culprits is not null)
                    {
                        data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(first));
                        if (data.ChangeBuffer.NeedCommit())
                        {
                            data.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(off, on, culprits));
                            if (StopOnFirstCommit) return true;
                        }
                    }
                    
                    queue.Enqueue(onFriend);
                }
            }
        }

        return false;
    }

    private List<CellPossibility>? FindCulprits(CellPossibility cp, BinairoSolver data,
        Dictionary<CellPossibility, CellPossibility> on, CellPossibility first)
    {
        return null; //TODO
    }

    private static bool IsOn(CellPossibility cp, BinairoSolver data, Dictionary<CellPossibility, CellPossibility> on,
        CellPossibility first)
    {
        return data[cp.Row, cp.Column] == cp.Possibility || on.ContainsKey(cp) || first == cp;
    }
}

public class NishioForcingNetReportBuilder : 
    IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter> //TODO 3 cases
{
    private readonly Dictionary<CellPossibility, CellPossibility> _off;
    private readonly Dictionary<CellPossibility, CellPossibility> _on;
    private readonly List<CellPossibility> _culprits;
    private readonly bool _isSameCell;
    private readonly ElementColor _color;

    public NishioForcingNetReportBuilder(Dictionary<CellPossibility, CellPossibility> off, 
        Dictionary<CellPossibility, CellPossibility> on, List<CellPossibility> culprits)
    {
        _off = off;
        _on = on;
        _culprits = culprits;
        _isSameCell = false;
    }

    public NishioForcingNetReportBuilder(Dictionary<CellPossibility, CellPossibility> off,
        Dictionary<CellPossibility, CellPossibility> on, CellPossibility cp)
    {
        _off = off;
        _on = on;
        _culprits = new List<CellPossibility>(1) {cp};
        _isSameCell = true;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }
}