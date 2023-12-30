using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Global.Enums;
using View.Utility;

namespace View.Pages.Player.UserControls;

public partial class HighlightColorsUserControl
{
    public event OnHighlightChosen? HighlightChosen;
    
    public HighlightColorsUserControl()
    {
        InitializeComponent();
        
        Initialize();
    }

    private void Initialize()
    {
        foreach (var color in Enum.GetValues<HighlightColor>())
        {
            UIElement element = color == HighlightColor.None
                ? new Image
                {
                    Source = new BitmapImage(new Uri("../../../Images/cross.png", UriKind.Relative)),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(0, 0, 5, 5)
                }
                : new Rectangle
                {
                    Fill = new SolidColorBrush(ColorUtility.ToColor(color)),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(0, 0, 5, 5)
                };

            element.MouseLeftButtonDown += (_, _) =>
            {
                HighlightChosen?.Invoke(color);
            };

            Panel.Children.Add(element);
        }
    }
}

public delegate void OnHighlightChosen(HighlightColor color);