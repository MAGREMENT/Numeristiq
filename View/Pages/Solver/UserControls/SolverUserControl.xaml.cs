using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;
using View.Utils;

namespace View.Pages.Solver.UserControls;

public partial class SolverUserControl : IHighlightable
{
    public const int CellSize = 60;
    private const int LineWidth = 3;

    private readonly SolverBackgroundManager _backgroundManager;

    public SolverUserControl()
    {
        InitializeComponent();

        //Init background
        _backgroundManager = new SolverBackgroundManager(CellSize, LineWidth);
        Main.Width = _backgroundManager.Size;
        Main.Height = _backgroundManager.Size;
        Main.Background = _backgroundManager.Background;
        
        //Init numbers
        for (int i = 0; i < 9; i++)
        {
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(LineWidth)
            });
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(CellSize)
            });
            var horizontal = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = (i + 1).ToString(),
                FontSize = 15,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(horizontal, 1 + i * 2);
            HorizontalNumbers.Children.Add(horizontal);
            
            VerticalNumbers.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(LineWidth)
            });
            VerticalNumbers.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(CellSize)
            });
            var vertical = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = (i + 1).ToString(),
                FontSize = 15,
                FontWeight = FontWeights.Bold
            };
            Grid.SetRow(vertical, 1 + i * 2);
            VerticalNumbers.Children.Add(vertical);
        }
        
        //Init cells
        for (int i = 0; i < 9; i++)
        {
            StackPanel row = (StackPanel)Main.Children[i];
            for (int j = 0; j < 9; j++)
            {
                var toAdd = new CellUserControl();
                toAdd.SetMargin(LineWidth, LineWidth, 0, 0);
                row.Children.Add(toAdd);

                int rowForEvent = i;
                int colForEvent = j;
                toAdd.MouseLeftButtonDown += (_, _) =>
                {
                    if (toAdd.IsFocused)
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(toAdd), null);
                        Keyboard.ClearFocus();
                    }
                    else toAdd.Focus();
                };

                toAdd.Focusable = true;
                toAdd.GotFocus += (_, _) =>
                {
                    _backgroundManager.PutCursorOn(rowForEvent, colForEvent);
                    Main.Background = _backgroundManager.Background;
                    var cell = new Cell(rowForEvent, colForEvent);
                };
                
                toAdd.LostFocus += (_, _) =>
                {
                    _backgroundManager.PutCursorOn(rowForEvent, colForEvent);
                    Main.Background = _backgroundManager.Background;
                };
            }
        }
    }

    public void SetCellTo(int row, int col, int number)
    {
        GetTo(row, col).SetNumber(number);
    }

    public void SetCellTo(int row, int col, int[] possibilities)
    {
        GetTo(row, col).SetPossibilities(possibilities);
    }

    public void UpdateGivens(HashSet<Cell> givens)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (givens.Contains(new Cell(row, col))) GetTo(row, col).SetForeground(CellForegroundType.Given);
                else GetTo(row, col).SetForeground(CellForegroundType.Solving);
            }
        }
    }

    public ChangeType ActionOnKeyboardInput { get; set; } = ChangeType.Solution;

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)Main.Children[row]).Children[col];
    }
    
    //IHighlightable----------------------------------------------------------------------------------------------------
    
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightPossibility(row, col, possibility, ColorManager.ToColor(coloration));
    }

    public void CirclePossibility(int possibility, int row, int col)
    {
        _backgroundManager.CirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightCell(row, col, ColorManager.ToColor(coloration));
    }

    public void CircleCell(int row, int col)
    {
        _backgroundManager.CircleCell(row, col);
    }

    public void HighlightLinkGraphElement(ILinkGraphElement element, ChangeColoration coloration)
    {
        switch (element)
        {
            case CellPossibility coord :
                _backgroundManager.HighlightPossibility(coord.Row, coord.Col, coord.Possibility, ColorManager.ToColor(coloration));
                break;
            case PointingRow pr :
                _backgroundManager.HighlightGroup(pr, ColorManager.ToColor(coloration));
                break;
            case PointingColumn pc :
                _backgroundManager.HighlightGroup(pc, ColorManager.ToColor(coloration));
                break;
            case AlmostNakedSet anp :
                _backgroundManager.HighlightGroup(anp, ColorManager.ToColor(coloration));
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _backgroundManager.CreateLink(from, to, linkStrength == LinkStrength.Weak);
    }

    public void CreateLink(ILinkGraphElement from, ILinkGraphElement to, LinkStrength linkStrength)
    {
        switch (from)
        {
            case CellPossibility one when to is CellPossibility two:
                _backgroundManager.CreateLink(one, two, linkStrength == LinkStrength.Weak);
                break;
            case CellPossibility when to is AlmostNakedSet:
                break;
            default:
                CellPossibility[] winners = new CellPossibility[2];
                double winningDistance = int.MaxValue;

                foreach (var c1 in from.EveryCellPossibilities())
                {
                    foreach (var c2 in to.EveryCellPossibilities())
                    {
                        foreach (var pc1 in c1.ToCellPossibility())
                        {
                            foreach (var pc2 in c2.ToCellPossibility())
                            {
                                var distance = Math.Pow(pc1.Row - pc2.Row, 2) + Math.Pow(pc1.Col - pc2.Col, 2);

                                if (distance < winningDistance)
                                {
                                    winningDistance = distance;
                                    winners[0] = pc1;
                                    winners[1] = pc2;
                                }
                            }
                        }
                    }
                }
            
                _backgroundManager.CreateLink(winners[0], winners[1], linkStrength == LinkStrength.Weak);
                break;
        }
    }
}

