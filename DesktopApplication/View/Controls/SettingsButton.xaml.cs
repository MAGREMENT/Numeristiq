using System.Windows.Input;
using System.Windows.Media;

namespace DesktopApplication.View.Controls;

public partial class SettingsButton
{
    public event OnClick? Clicked;
    
    public SettingsButton()
    {
        InitializeComponent();
        
        RenderOptions.SetBitmapScalingMode(SettingImage, BitmapScalingMode.Fant);
    }

    private void ShowSettingWindow(object sender, MouseButtonEventArgs e)
    {
        Clicked?.Invoke();
    }
}

public delegate void OnClick();