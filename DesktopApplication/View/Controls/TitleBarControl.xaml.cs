using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shell;

namespace DesktopApplication.View.Controls;

[ContentProperty("InsideContent")]
public partial class TitleBarControl
{
    public event TitleBarAction? Minimize;
    public event TitleBarAction? ChangeSize;
    public event TitleBarAction? Close;

    private bool _allowResize = true;
    
    public object InsideContent
    {
        get => GetValue(AdditionalContentProperty);
        set => SetValue(AdditionalContentProperty, value);
    }
    
    public static readonly DependencyProperty AdditionalContentProperty =
        DependencyProperty.Register("InsideContent", typeof(object), typeof(TitleBarControl),
            new PropertyMetadata(null));

    public bool AllowResize
    {
        set
        {
            _allowResize = value;
            if (value == false)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
        }
    }

    public ImageSource AppIcon
    {
        set
        {
            Icon.Source = value;
            Icon.Visibility = Visibility.Visible;
        }
    }

    public string AppTitle
    {
        set
        {
            Title.Text = value;
            Title.Visibility = Visibility.Visible;
        } 
    }
    
    public TitleBarControl()
    {
        InitializeComponent();
        
        RenderOptions.SetBitmapScalingMode(Icon, BitmapScalingMode.Fant);
        WindowChrome.SetIsHitTestVisibleInChrome(ContentControl, true);
    }

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        Minimize?.Invoke();
    }

    private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
    {
        if (_allowResize) ChangeSize?.Invoke();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close?.Invoke();     
    }
    
    public void RefreshMaximizeRestoreButton(WindowState state)
    {
        if (!_allowResize) return;
        
        if (state == WindowState.Maximized)
        {
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreButton.Visibility = Visibility.Visible;
        }
        else
        {
            MaximizeButton.Visibility = Visibility.Visible;
            RestoreButton.Visibility = Visibility.Collapsed;
        }
    }
}

public delegate void TitleBarAction();