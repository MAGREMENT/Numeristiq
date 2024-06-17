namespace DesktopApplication.Presenter;

public class Disabler
{
    private readonly ICanBeDisabled _target;
    private ulong _currentlyDisabling;

    public Disabler(ICanBeDisabled target)
    {
        _target = target;
    }

    public void Enable(int id)
    {
        _currentlyDisabling &= ~(1ul << id);
        if(System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _target.Enable();
    }

    public void Disable(int id)
    {
        if (System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _target.Disable();
        _currentlyDisabling |= 1ul << id;
    }
}

public interface ICanBeDisabled
{
    void Enable();
    void Disable();
}