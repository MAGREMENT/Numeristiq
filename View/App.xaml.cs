using View.Themes;

namespace View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        public event ApplyTheme? ThemeChanged;
        
        public App()
        {
            InitializeComponent();
        }
        
        public void ChangeTheme(Theme theme)
        {
            var r = Resources;
            r.Clear();
            r.Add("Background1", theme.Background1);
            r.Add("Background2", theme.Background2);
            r.Add("Background3", theme.Background3);
            r.Add("Primary1", theme.Primary1);
            r.Add("Primary2", theme.Primary2);
            r.Add("Secondary1", theme.Secondary1);
            r.Add("Secondary2", theme.Secondary2);
            r.Add("Accent", theme.Accent);
            r.Add("Border", theme.Border);
            r.Add("Text", theme.Text);

            ThemeChanged?.Invoke(theme);
        }
    }
}