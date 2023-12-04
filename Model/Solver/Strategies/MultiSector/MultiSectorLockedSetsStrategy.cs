using System.Collections.Generic;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies.MultiSector;

public class MultiSectorLockedSetsStrategy : AbstractStrategy
{
    public const string OfficialName = "Multi-Sector Locked Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IMultiSectorCellsSearcher[] _cellsSearchers;
    
    public MultiSectorLockedSetsStrategy(params IMultiSectorCellsSearcher[] searchers) : base(OfficialName,
        StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _cellsSearchers = searchers;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        List<PossibilityCovers> covers = new();
        foreach (var searcher in _cellsSearchers)
        {
            foreach (var grid in searcher.SearchGrids(strategyManager))
            {
                var count = 0;
                
                for (int number = 1; number <= 9; number++)
                {
                    var positions = strategyManager.PositionsFor(number);
                    var and = grid.And(positions);
                    if (and.Count == 0) continue;

                    var coverHouses = and.BestCoverHouses(UnitMethods.All);
                    count += coverHouses.Count;
                    covers.Add(new PossibilityCovers(number, coverHouses.ToArray()));
                }

                if (count == grid.Count && Process(strategyManager, grid, covers)) return;
                covers.Clear();
            }
        }
    }

    private bool Process(IStrategyManager strategyManager, GridPositions grid, List<PossibilityCovers> covers)
    {
        foreach (var cover in covers)
        {
            foreach (var house in cover.CoverHouses)
            {
                var method = UnitMethods.GetMethods(house.Unit);
                foreach (var cell in method.EveryCell(house.Number))
                {
                    if (grid.Peek(cell)) continue;
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(cover.Possibility, cell);
                }
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new MultiSectorLockedSetsReportBuilder(grid, covers)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public interface IMultiSectorCellsSearcher
{
    public IEnumerable<GridPositions> SearchGrids(IStrategyManager strategyManager);
}

public record PossibilityCovers(int Possibility, CoverHouse[] CoverHouses);

public class MultiSectorLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _grid;
    private readonly List<PossibilityCovers> _covers;

    public MultiSectorLockedSetsReportBuilder(GridPositions grid, List<PossibilityCovers> covers)
    {
        _grid = grid;
        _covers = covers;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var cu = new CoveringUnits(_covers);
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(cu), lighter =>
        {
            foreach (var cell in _grid)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, cu.Highlight(snapshot, _grid));
    }

    private string Explanation(CoveringUnits cu)
    {
        var builder = new StringBuilder("Cells : ");

        int count = 0;
        foreach (var cell in _grid)
        {
            builder.Append(count++ == _grid.Count - 1 ? cell.ToString() : cell + ", ");
        }

        builder.Append("\nLinks : " + cu);
        return builder.ToString();
    }
}

public class CoveringUnits
{
    private readonly Dictionary<CoverHouse, Possibilities> _dictionary = new();

    public CoveringUnits(List<PossibilityCovers> list)
    {
        foreach (var cover in list)
        {
            foreach (var house in cover.CoverHouses)
            {
                if (!_dictionary.TryGetValue(house, out var poss))
                {
                    poss = Possibilities.NewEmpty();
                    _dictionary[house] = poss;
                }

                poss.Add(cover.Possibility);
            }
        }
    }

    public Highlight[] Highlight(IPossibilitiesHolder snapshot, GridPositions gp)
    {
        var n = (int)ChangeColoration.CauseOffOne;
        var i = 0;
        List<Highlight> h = new();

        foreach (var entry in _dictionary)
        {
            var color = (ChangeColoration)(n + i);
            
            h.Add(lighter =>
            {
                foreach (var cell in gp)
                {
                    lighter.HighlightCell(cell, ChangeColoration.Neutral);
                }
                
                lighter.EncircleRectangle(entry.Key, color);
                foreach (var cell in UnitMethods.GetMethods(entry.Key.Unit).EveryCell(entry.Key.Number))
                {
                    if (!gp.Peek(cell)) continue;
                    
                    foreach (var p in entry.Value)
                    {
                        if(snapshot.PossibilitiesAt(cell).Peek(p)) lighter.HighlightPossibility(p, cell.Row, cell.Column, color);
                    }
                }
            });
            
            i = (i + 1) % 10;
        }

        return h.ToArray();
    }

    public override string ToString()
    {
        var result = new StringBuilder();

        foreach (var entry in _dictionary)
        {
            foreach (var p in entry.Value)
            {
                result.Append(p);
            }

            result.Append(entry.Key + ", ");
        }

        return result.ToString()[..^2];
    }
}