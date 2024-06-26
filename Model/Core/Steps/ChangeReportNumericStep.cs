﻿using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public class ChangeReportNumericStep<THighlighter> : INumericStep<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public StepDifficulty Difficulty { get; }
    public IReadOnlyList<NumericChange> Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public INumericSolvingState From { get; }
    public INumericSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager  { get; }


    public ChangeReportNumericStep(int id, Strategy maker, IReadOnlyList<NumericChange> changes, ChangeReport<THighlighter> report,
        INumericSolvingState stateBefore, INumericSolvingState stateAfter)
    {
        Id = id;
        Title = maker.Name;
        Difficulty = maker.Difficulty;
        Changes = changes;
        Description = report.Description;
        From = stateBefore;
        To = stateAfter;
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}