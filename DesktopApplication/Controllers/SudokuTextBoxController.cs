namespace DesktopApplication.Controllers;

public class SudokuTextBoxController
{
    private readonly ISudokuTextBoxView _textBox;
    private readonly IControllerCallback _callback;

    public SudokuTextBoxController(IControllerCallback callback, ISudokuTextBoxView textBox)
    {
        _textBox = textBox;
        _callback = callback;
    }

    public void OnShow()
    {
        _textBox.SetText(_callback.GetSudokuAsString());
    }
}

public interface ISudokuTextBoxView
{
    public void SetText(string s);
}