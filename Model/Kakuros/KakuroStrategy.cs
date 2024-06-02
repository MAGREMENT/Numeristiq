using Model.Core;

namespace Model.Kakuros;

public abstract class KakuroStrategy : Strategy
{ 
    protected KakuroStrategy(string name, StepDifficulty difficulty, InstanceHandling instanceHandling)
        : base(name, difficulty, instanceHandling)
    {
        Name = name;
        Difficulty = difficulty;
        InstanceHandling = instanceHandling;
    }

    public abstract void Apply(IKakuroStrategyUser strategyUser);
}