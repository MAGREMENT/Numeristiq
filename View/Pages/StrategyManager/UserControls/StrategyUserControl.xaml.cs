using System.Windows;
using System.Windows.Media;
using Presenter.Translator;
using View.Utility;

namespace View.Pages.StrategyManager.UserControls;

public partial class StrategyUserControl
{
    private const double LineWidth = 3;
    
    public event OnStrategyAddition? StrategyAdded;
    public event OnStrategiesInterchange? StrategiesInterchanged;
    
    public StrategyUserControl(ViewStrategy strategy, int position)
    {
        InitializeComponent();

        TextBlock.Foreground = new SolidColorBrush(ColorManager.ToColor(strategy.Intensity));
        TextBlock.Text = strategy.Name;

        AllowDrop = true;
        MouseDown += (_, _) =>
        {
            DragDrop.DoDragDrop(this, new StrategyShuffleData(TextBlock.Text, false, position),
                DragDropEffects.All);
        };
        Drop += (_, args) =>
        {
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;

            var relativePosition = args.GetPosition(this).Y > ActualHeight / 2 ? position + 1 : position;
            if (data.FromSearch) StrategyAdded?.Invoke(data.Name, relativePosition);
            else if (data.CurrentPosition != -1) StrategiesInterchanged?.Invoke(data.CurrentPosition, relativePosition);

            args.Handled = true;
        };
        DragOver += (_, args) =>
        {
            var pos = args.GetPosition(this);
            Background = DragBackground(pos.Y <= ActualHeight  / 2);
        };
        DragLeave += (_, _) =>
        {
            Background = ColorManager.Background1;
        };
    }

    private Brush DragBackground(bool upper)
    {
        var y = upper ? 0 : ActualHeight - LineWidth - 1;
        var group = new DrawingGroup();
        
        group.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)),
            Brush = Brushes.Transparent
        });
        
        group.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(0, y, ActualWidth, LineWidth)),
            Pen = new Pen
            {
                Thickness = LineWidth,
                Brush = Brushes.Aqua
            }
        });
        
        var brush = new DrawingBrush
        {
            TileMode = TileMode.None,
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Drawing = group
        };

        return brush;
    }
}