using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.YourPuzzles;
using DesktopApplication.View.Controls;
using Model.Utility;
using Model.YourPuzzles;

namespace DesktopApplication.View.YourPuzzles.Pages;

public partial class CreatePage : IYourPuzzleView
{
    private readonly YourPuzzlePresenter _presenter;

    private readonly StackPanel _currentRulesList = new();

    private readonly TextBox _currentRulesText = new()
    {
        FontSize = 15,
        FontWeight = FontWeights.Bold
    };
    
    public CreatePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
    }

    public IYourPuzzleDrawer Drawer => (IYourPuzzleDrawer)Embedded.OptimizableContent!;
    public void ClearRulesInBank()
    {
        GlobalBank.Children.Clear();
        LocalBank.Children.Clear();
    }

    public void AddRuleInBank(RuleBankSearchResult result)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });

        var ellipse = new Ellipse
        {
            Width = 10,
            Height = 10,
            Margin = new Thickness(5, 3, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left 
        };
        ellipse.SetResourceReference(Shape.FillProperty, "Primary");
        Grid.SetColumn(ellipse, 0);
        grid.Children.Add(ellipse);

        var tb = new TextBlock
        {
            Text = result.Name,
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        tb.SetResourceReference(ForegroundProperty, result.CanBeCrafted ? "Text" : "Disabled");
        Grid.SetColumn(tb, 1);

        if (result.CanBeCrafted)
        {
            var path = new Path
            {
                StrokeThickness = 2,
                Width = 10,
                Height = 10,
                Data = Geometry.Parse("M 1,5 H 9 M 5,1 V 9")
            };
            path.SetResourceReference(Shape.StrokeProperty, "Text");
            
            var add = new Button
            {
                Style = (Style)FindResource("PrimaryRoundedButton"),
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Content = path
            };
            add.Click += (_, _) => _presenter.AddRule(result.Index, result.IsGlobal);

            Grid.SetColumn(add, 2);
            grid.Children.Add(add);
        }
        
        grid.Children.Add(tb);
        var button = new Button
        {
            Content = grid,
            IsEnabled = result.CanBeCrafted,
            Style = (Style)FindResource("RuleButton"),
        };

        if (result.IsGlobal) GlobalBank.Children.Add(button);
        else LocalBank.Children.Add(button);
    }

    public void ClearCurrentRules()
    {
        _currentRulesList.Children.Clear();
        _currentRulesText.Text = "";
    }

    public void AddCurrentRule(INumericPuzzleRule rule, int index, bool isGlobal)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });

        var ellipse = new Ellipse
        {
            Width = 10,
            Height = 10,
            Margin = new Thickness(5, 3, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left 
        };
        ellipse.SetResourceReference(Shape.FillProperty, "Primary");
        Grid.SetColumn(ellipse, 0);
        grid.Children.Add(ellipse);

        var tb = new TextBlock
        {
            Text = rule.Name,
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        tb.SetResourceReference(ForegroundProperty, "Text");
        Grid.SetColumn(tb, 1);

        var path = new Path
        {
            StrokeThickness = 2,
            Width = 10,
            Height = 10,
            Data = Geometry.Parse("M 1,5 H 9")
        };
        path.SetResourceReference(Shape.StrokeProperty, "Text");
            
        var remove = new Button
        {
            Style = (Style)FindResource("PrimaryRoundedButton"),
            Margin = new Thickness(5, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Content = path
        };
        remove.Click += (_, _) => _presenter.RemoveRule(index, isGlobal);

        Grid.SetColumn(remove, 2);
        grid.Children.Add(remove);
        
        grid.Children.Add(tb);
        var button = new Button
        {
            Content = grid,
            Style = (Style)FindResource("RuleButton"),
        };

        _currentRulesList.Children.Add(button);

        /*var syntax = rule.ToSyntax();
        var elements = syntax.EnumerateLeftToRight().ToList();
        for (int i = 0; i < elements.Count; i++)
        {
            if(i != 0) _currentRulesText.Inlines.Add(new Run(" "));
            var str = elements[i].ToSyntaxString();
            _currentRulesText.Inlines.Add(new Run(str.value)
            {
                Foreground = App.Current.ThemeInformation.ToBrush(str.color)
            });
        }
        
        _currentRulesText.Inlines.Add(new Run("\n"));*/
    }

    public void SetRuleType(bool list)
    {
        if (list)
        {
            CurrentRuleContainer.Content = _currentRulesList;
            TextOption.SetResourceReference(TextBlock.ForegroundProperty, "Text");
            ListOption.SetResourceReference(TextBlock.ForegroundProperty, "Primary");
        }
        else
        {
            CurrentRuleContainer.Content = _currentRulesText;
            TextOption.SetResourceReference(TextBlock.ForegroundProperty, "Primary");
            ListOption.SetResourceReference(TextBlock.ForegroundProperty, "Text");
        }
    }

    public void SetYourPuzzleString(string s)
    {
        PuzzleString.SetText(s);
    }

    public override string Header => "Create";
    public override void OnShow()
    {
        
    }

    public override void OnClose()
    {
        
    }

    public override object? TitleBarContent()
    {
        return null;
    }

    private void OnRowDimensionChangeAsked(int diff)
    {
        _presenter.SetRowCount(diff);
    }

    private void OnColumnDimensionChangeAsked(int diff)
    {
        _presenter.SetColumnCount(diff);
    }

    private void SelectCell(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void AddCellToSelection(int row, int col)
    {
        _presenter.AddCellToSelection(new Cell(row, col));
    }

    private void UpdateRowCount(int number)
    {
        ((DimensionChooser)Embedded.SideControls[0]!).SetDimension(number);
    }
    
    private void UpdateColumnCount(int number)
    {
        ((DimensionChooser)Embedded.SideControls[1]!).SetDimension(number);
    }

    private void OnShowed()
    {
        _presenter.ShowPuzzleString();
    }

    private void OnTextChange(string s)
    {
        _presenter.OnNewPuzzle(s);
    }

    private void OnListClick(object sender, MouseButtonEventArgs e)
    {
        SetRuleType(true);
    }
    
    private void OnTextClick(object sender, MouseButtonEventArgs e)
    {
        SetRuleType(false);
    }
}