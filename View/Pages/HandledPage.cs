using System.Windows.Controls;


namespace View.Pages;

public abstract class HandledPage : Page
{
    public abstract void OnShow();
    public abstract void OnQuit();
}