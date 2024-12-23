﻿using Model.Binairos;
using Model.Binairos.Strategies;
using Model.Core;

namespace Repository.HardCoded;

public class HardCodedBinairoStrategyRepository : HardCodedStrategyRepository<Strategy<IBinairoSolverData>>
{
    public override IEnumerable<Strategy<IBinairoSolverData>> GetStrategies()
    {
        yield return new DoubleStrategy();
        yield return new TripleDenialStrategy();
        yield return new HalfCompletionStrategy();
        yield return new UniquenessEnforcementStrategy();
        yield return new AdvancedTripleDenialStrategy();
        yield return new AdvancedUniquenessEnforcementStrategy();
        yield return new AlternatingInferenceChainStrategy();
    }
}