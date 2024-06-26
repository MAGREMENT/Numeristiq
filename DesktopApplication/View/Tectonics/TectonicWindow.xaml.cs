﻿using System.Windows;
using DesktopApplication.Presenter;
using DesktopApplication.View.Tectonics.Pages;

namespace DesktopApplication.View.Tectonics;

public partial class TectonicWindow
{
    public TectonicWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        var presenter = GlobalApplicationPresenter.Instance.InitializeTectonicApplicationPresenter();

        Frame.Content = new SolvePage(presenter);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}