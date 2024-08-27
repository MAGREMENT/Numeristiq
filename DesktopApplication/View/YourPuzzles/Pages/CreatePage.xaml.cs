using System.Windows;
using System.Windows.Controls;
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
        ellipse.SetResourceReference(Shape.FillProperty, "Primary1");
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
                Template = (ControlTemplate)FindResource("RoundedButton"),
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Content = path
            };
            add.SetResourceReference(BackgroundProperty, "Primary1");
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
        CurrentRules.Children.Clear();
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
        ellipse.SetResourceReference(Shape.FillProperty, "Primary1");
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
            Template = (ControlTemplate)FindResource("RoundedButton"),
            Margin = new Thickness(5, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Content = path
        };
        remove.SetResourceReference(BackgroundProperty, "Primary1");
        remove.Click += (_, _) => _presenter.RemoveRule(index, isGlobal);

        Grid.SetColumn(remove, 2);
        grid.Children.Add(remove);
        
        grid.Children.Add(tb);
        var button = new Button
        {
            Content = grid,
            Style = (Style)FindResource("RuleButton"),
        };

        CurrentRules.Children.Add(button);
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
}