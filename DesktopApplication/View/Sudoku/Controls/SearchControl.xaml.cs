using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SearchControl
{
    private bool _hasBoxContent;
    private bool _raiseEvent = true;

    public event OnSearch? Searched;
    
    public SearchControl()
    {
        InitializeComponent();

        Box.Focusable = true;
        Box.GotFocus += (_, _) =>
        {
            _raiseEvent = false;
            
            if (!_hasBoxContent) Box.Text = "";

            _raiseEvent = true;
        };

        Box.LostFocus += (_, _) =>
        {
            _raiseEvent = false;

            if (string.IsNullOrWhiteSpace(Box.Text))
            {
                Box.Text = "Search";
                _hasBoxContent = false;
            }
            else _hasBoxContent = true;
            
            _raiseEvent = true;
        };
    }

    private void Box_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if(_raiseEvent) Searched?.Invoke(Box.Text);
    }

    public void ClearResult()
    {
        ResultPanel.Children.Clear();
    }

    public void AddResult(string s)
    {
        var tb = new TextBlock
        {
            Text = s,
            Style = (Style)((App)Application.Current).Resources["SearchResult"]!
        };
        tb.MouseMove += (_, args) =>
        {
            if(args.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(tb, new StrategyDragDropData(s, -1), DragDropEffects.Move);
        };
        
        ResultPanel.Children.Add(tb);
    }
}

public delegate void OnSearch(string s);

public record StrategyDragDropData(string Name, int Index);