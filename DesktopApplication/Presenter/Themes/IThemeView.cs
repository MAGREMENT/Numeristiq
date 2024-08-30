using System.Collections.Generic;
using Model.Repositories;
using Model.Utility;

namespace DesktopApplication.Presenter.Themes;

public interface IThemeView
{
    void SetCurrentTheme(string name);
    void SetOtherThemes(IEnumerable<(Theme, bool)> themes);
    void SetColors(IEnumerable<(string, RGB)> colors, bool canBeSelected);
    void SelectColor(string name, RGB value);
    void UnselectColor();
    void ShowNameError(string error);
    void ShowNameIsCorrect();
    void SetContinuousUpdate(bool yes);
    void RedrawExampleGrid();
}