using Model.YourPuzzles;

namespace DesktopApplication.Presenter.YourPuzzles;

public interface IYourPuzzleView
{
    IYourPuzzleDrawer Drawer { get; }

    void ClearRulesInBank();
    void AddRuleInBank(RuleBankSearchResult result);
    void ClearCurrentRules();
    void AddCurrentRule(INumericPuzzleRule rule, int index, bool isGlobal);
}