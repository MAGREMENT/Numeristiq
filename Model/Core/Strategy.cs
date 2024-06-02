namespace Model.Core;

public abstract class Strategy
{
    private bool _enabled = true;
    
    public string Name { get; protected init; }
    public StepDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public InstanceHandling InstanceHandling { get; set; }
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (!Locked) _enabled = value;
        }
    }

    public bool Locked { get; set; }

    public bool StopOnFirstPush => InstanceHandling == InstanceHandling.FirstOnly;
    
    protected Strategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        InstanceHandling = defaultHandling;
    }
}

public enum StepDifficulty
{
    None, Basic, Easy, Medium, Hard, Extreme, Inhuman, ByTrial
}

public enum UniquenessDependency
{
    NotDependent, PartiallyDependent, FullyDependent
}

public enum InstanceHandling
{
    FirstOnly, UnorderedAll, BestOnly, SortedAll
}