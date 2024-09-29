using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Themes;

namespace DesktopApplication.View.HelperWindows.Dialog;

public partial class NameChooserDialog
{
    private readonly INameEvaluator _evaluator;
    private readonly bool _initialized;
    
    public event OnNameChosen? NameChosen;
    
    public NameChooserDialog(INameEvaluator evaluator)
    {
        InitializeComponent();
        
        _evaluator = evaluator;
        _initialized = true;
        UpdateStatus(default!, default!);
    }

    private void UpdateStatus(object sender, TextChangedEventArgs e)
    {
        if (!_initialized) return;
        if (_evaluator.IsNameCorrect(NameBox.Text, out var error))
        {
            NameFeedback.Text = "This name is valid";
            NameFeedback.SetResourceReference(ForegroundProperty, "On");
            NameButton.IsEnabled = true;
        }
        else
        {
            NameFeedback.Text = error;
            NameFeedback.SetResourceReference(ForegroundProperty, "Off");
            NameButton.IsEnabled = false;
        }
    }

    private void Validate(object sender, RoutedEventArgs e)
    {
        NameChosen?.Invoke(NameBox.Text);
        CloseOnDeactivate = false;
        Close();
    }
}

public delegate void OnNameChosen(string name);