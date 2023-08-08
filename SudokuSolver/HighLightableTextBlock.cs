using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace SudokuSolver;

public class HighlightableTextBlock : TextBlock
{
    public Color DefaultColor { get; set; } = Colors.White;
    
    private readonly List<Color> _currentColors = new();

    public void Highlight(Color color)
    {
        _currentColors.Add(color);
        if (_currentColors.Count == 1) Background = new SolidColorBrush(color);
        else
        {
            var part = Width / (_currentColors.Count - 1);
            GradientStopCollection coll = new();
            for (int i = 0; i < _currentColors.Count; i++)
            {
                coll.Add(new GradientStop(_currentColors[i], i * part / Width));
            }

            Background = new LinearGradientBrush(coll);
        }
    }

    public void UnHighlight()
    {
        _currentColors.Clear();
        Background = new SolidColorBrush(DefaultColor);
    }
}