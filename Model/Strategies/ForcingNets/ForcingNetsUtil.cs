using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingNets;

public static class ForcingNetsUtil
{
    public static Dictionary<PossibilityCoordinate, Coloring> FilterPossibilityCoordinates(
        Dictionary<ILinkGraphElement, Coloring> coloring)
    {
        Dictionary<PossibilityCoordinate, Coloring> result = new();
        foreach (var element in coloring)
        {
            if (element.Key is not PossibilityCoordinate coord) continue;
            result.Add(coord, element.Value);
        }

        return result;
    }

    public static void HighlightColoring(IHighlightable lighter, Dictionary<PossibilityCoordinate, Coloring> coloring)
    {
        foreach (var element in coloring)
        {
            lighter.HighlightPossibility(element.Key, element.Value == Coloring.On ? ChangeColoration.CauseOnOne :
                ChangeColoration.CauseOffOne);
        }
    }
}