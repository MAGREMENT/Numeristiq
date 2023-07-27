using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SudokuSolver;

public partial class LiveModificationUserControl : UserControl //TODO disable when looking back at logs
{
    private readonly NumbersUserControl _numbers;
    private readonly RadioButton _definitiveNumber;
    private readonly RadioButton _possibilities;

    private SudokuCellUserControl? _current = null;
    private int[] _currentPos = new int[2];

    private SudokuUserControl? _solverAccess = null;

    public LiveModificationUserControl()
    {
        InitializeComponent();
        
        _numbers = (FindName("Numbers") as NumbersUserControl)!;

        _numbers.SetSize(150);
        _numbers.Focusable = true;
        _numbers.Background = new SolidColorBrush(Colors.WhiteSmoke);
        _numbers.LostFocus += (_, _) =>
        {
            _numbers.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
        _numbers.GotFocus += (_, _) =>
        {
            _numbers.Background = new SolidColorBrush(Colors.Aqua);
        };
        _numbers.MouseDown += (_, _) =>
        {
            _numbers.Focus();
        };

        _definitiveNumber = (FindName("A") as RadioButton)!;
        _possibilities = (FindName("B") as RadioButton)!;
    }

    public void Init(SudokuUserControl suc)
    {
        _solverAccess = suc;
    }

    public void SetCurrent(SudokuCellUserControl scuc, int row, int col)
    {
        if (_current is not null) _current.Updated -= Update;
        if (_current == scuc)
        {
            _current = null;
            return;
        }
        _current = scuc;
        _currentPos[0] = row;
        _currentPos[1] = col;

        _current.Updated += Update;
        _current.FireUpdated();
        _numbers.Focus();
    }

    private void Update(bool isPossibilities, int[] numbers)
    {
        if (_current is not null)
        {
            if (isPossibilities) _numbers.SetSmall(numbers);
            else _numbers.SetBig(numbers[0]);
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
        if (_solverAccess is not null && _current is not null)
        {
            if (_definitiveNumber.IsChecked == true)
                _solverAccess.AddDefinitiveNumber(i, _currentPos[0], _currentPos[1]);
            if (_possibilities.IsChecked == true)
                _solverAccess.RemovePossibility(i, _currentPos[0], _currentPos[1]); 
        }
    }
}