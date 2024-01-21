using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Global;
using Global.Enums;
using Model;
using Model.Solver.Explanation;
using Model.Solver.Helpers.Logs;
using Model.Solver.StrategiesUtility;
using View.Utility;

namespace View.HelperWindows.Explanation;

public partial class ExplanationWindow : IExplanationShower
{
    public ExplanationWindow()
    {
        InitializeComponent();
    }

    public void ShowExplanation(ISolverLog log)
    {
        Grid.ClearNumber();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = log.StateBefore.At(row, col);
                if(current.IsPossibilities) Grid.SetCellTo(row, col, current.AsPossibilities.ToArray(), CellColor.Black);
                else Grid.SetCellTo(row, col, current.AsNumber, CellColor.Black);
            }
        }
        Grid.Refresh();
        
        Explanation.Inlines.Clear();
        if (log.Explanation is null)
        {
            Explanation.Text = "There are no explanation for this log";
            return;
        }
        
        var e = log.Explanation;
        do
        {
            AddExplanationElement(Explanation, e);
            e = e.Next;
        } while (e is not null);
    }

    private void AddExplanationElement(TextBlock tb, ExplanationElement element)
    {
        var r = new Run(element.ToString());
        if(element.ShouldBeBold) r.FontWeight = FontWeights.Bold;
        r.Foreground = ColorUtility.ToBrush(element.Color);
        r.MouseLeftButtonDown += (_, _) => element.Show(this);
        tb.Inlines.Add(r);
    }

    public void ShowCell(Cell c)
    {
        Grid.ClearDrawings();
        Grid.FillCell(c.Row, c.Column, ChangeColoration.CauseOffOne);
        Grid.Refresh();
    }

    public void ShowCellPossibility(CellPossibility cp)
    {
        Grid.ClearDrawings();
        Grid.FillPossibility(cp.Row, cp.Column, cp.Possibility, ChangeColoration.CauseOffOne);
        Grid.Refresh();
    }

    public void ShowCoverHouse(CoverHouse ch)
    {
        Grid.ClearDrawings();
        Cell min = default;
        Cell max = default;
        switch (ch.Unit)
        {
            case Unit.Row :
                min = new Cell(ch.Number, 0);
                max = new Cell(ch.Number, 8); 
                break;
            case Unit.Column :
                min = new Cell(0, ch.Number);
                max = new Cell(8, ch.Number);
                break;
            case Unit.MiniGrid :
                var sRow = ch.Number / 3 * 3;
                var sCol = ch.Number % 3 * 3;
                min = new Cell(sRow, sCol);
                max = new Cell(sRow + 2, sCol + 2);
                break;
        }

        Grid.EncircleRectangle(min.Row, min.Column, max.Row, max.Column, ChangeColoration.CauseOffOne);
        Grid.Refresh();
    }
}