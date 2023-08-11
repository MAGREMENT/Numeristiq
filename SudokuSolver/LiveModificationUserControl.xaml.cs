using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Model;
using Model.Possibilities;

namespace SudokuSolver;

public partial class LiveModificationUserControl //TODO disable when looking back at logs
{

    private CellUserControl? _current;
    private readonly int[] _currentPos = new int[2];

    public delegate void OnLiveModification(int number, int row, int col, SolverNumberType numberType);
    public event OnLiveModification? LiveModified;

    public LiveModificationUserControl()
    {
        InitializeComponent();

        Cell.SetSize(150);
        Cell.Focusable = true;
        Cell.Background = new SolidColorBrush(Colors.WhiteSmoke);
        Cell.LostFocus += (_, _) =>
        {
            Cell.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
        Cell.GotFocus += (_, _) =>
        {
            Cell.Background = new SolidColorBrush(Colors.Aqua);
        };
        Cell.MouseDown += (_, _) =>
        {
            Cell.Focus();
        };
    }

    public void SetCurrent(CellUserControl scuc, int row, int col)
    {
        if (_current is not null) _current.Updated -= Update;
        if (_current == scuc)
        {
            _current = null;
            
            // Kill logical and keyboard focus
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(Cell), null);
            Keyboard.ClearFocus();
            
            Cell.Void();
            return;
        }
        
        _current = scuc;
        _currentPos[0] = row;
        _currentPos[1] = col;

        _current.Updated += Update;
        _current.FireUpdated();
        Cell.Focus();
    }

    private void Update(bool isPossibilities, IPossibilities numbers)
    {
        if (_current is not null)
        {
            if (isPossibilities) Cell.SetPossibilities(numbers);
            else Cell.SetDefinitiveNumber(numbers.GetFirst());
        }
    }

    private void KeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.NumPad1 : LiveModification(1);
                break;
            case Key.NumPad2 : LiveModification(2);
                break;
            case Key.NumPad3 : LiveModification(3);
                break;
            case Key.NumPad4 : LiveModification(4);
                break;
            case Key.NumPad5 : LiveModification(5);
                break;
            case Key.NumPad6 : LiveModification(6);
                break;
            case Key.NumPad7 : LiveModification(7);
                break;
            case Key.NumPad8 : LiveModification(8);
                break;
            case Key.NumPad9 : LiveModification(9);
                break;
        }
    }

    private void LiveModification(int i)
    {
        if (_current is not null)
        {
            SolverNumberType numberType;
            if (A.IsChecked == true) numberType = SolverNumberType.Definitive;
            else if (B.IsChecked == true) numberType = SolverNumberType.Possibility;
            else return;
            
            LiveModified?.Invoke(i, _currentPos[0], _currentPos[1], numberType);
        }
    }

    private void FocusThis(object sender, RoutedEventArgs e)
    {
        Cell.Focus();
    }
}