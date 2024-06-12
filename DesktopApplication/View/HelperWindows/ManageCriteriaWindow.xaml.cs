using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Settings;
using Model.Helpers.Settings;
using Model.Sudokus.Generator;

namespace DesktopApplication.View.HelperWindows;

public partial class ManageCriteriaWindow : IManageCriteriaView
{
    private readonly ManageCriteriaPresenter _presenter;
    private bool _isSaved = false;
    
    public ManageCriteriaWindow(ManageCriteriaPresenterBuilder builder)
    {
        InitializeComponent();

        _presenter = builder.Build(this);
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
        Closed += (_, _) =>
        {
            if (!_isSaved) _presenter.CancelChanges();
        };
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    public void AddSearchResult(string s)
    {
        var tb = new TextBlock
        {
            Text = s,
            Style = (Style)FindResource("SearchResult")
        };
        tb.MouseLeftButtonDown += (_, _) => _presenter.SelectCriteriaFromSearch(s);
        
        Search.AddResult(tb);
    }

    public void SetSelectedCriteria(EvaluationCriteria criteria)
    {
        CriteriaName.Text = criteria.Name;
        
        CriteriaOptions.Children.Clear();
        for (int i = 0; i < criteria.Settings.Count; i++)
        {
            var element = SettingTranslator.Translate(criteria, criteria.Settings[i], i);
            if (element is not null)
            {
                element.AutoSet = true;
                element.Margin = new Thickness(5, 0, 0, 10);
                CriteriaOptions.Children.Add(element);
            }
        }
    }

    public void SetButtonAction(bool toAdd)
    {
        Action.Visibility = Visibility.Visible;
        if (toAdd)
        {
            Action.Content = "Add";
            Action.Style = (Style)Action.FindResource("FullPrimaryButton");
        }
        else
        {
            Action.Content = "Remove";
            Action.Style = (Style)Action.FindResource("FullSecondaryButton");
        }
    }

    public void SetCriteriaList(IReadOnlyList<EvaluationCriteria> criteriaList)
    {
        Criterias.Children.Clear();
        for (int i = 0; i < criteriaList.Count; i++)
        {
            var control = new EvaluationCriteriaControl(criteriaList[i]);
            var iForEvent = i;
            control.Clicked += () => _presenter.SelectCriteriaFromList(iForEvent);

            Criterias.Children.Add(control);
        }
    }

    public void UpdateCriteriaSettings(int index, IReadOnlyList<IReadOnlySetting> settings)
    {
        if (Criterias.Children[index] is not EvaluationCriteriaControl control) return;

        control.UpdateSettings(settings);
    }

    private void DoButtonAction(object sender, RoutedEventArgs e)
    {
        _presenter.DoCriteriaAction();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        _isSaved = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
}