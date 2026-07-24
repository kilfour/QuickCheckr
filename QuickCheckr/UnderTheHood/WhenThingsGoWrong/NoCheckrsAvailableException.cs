using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class NoCheckrsAvailableException()
    : QuickCheckrException(
        @"Checkr.OneOfWhen could not choose a Checkr because none of the conditions evaluated to true.

OneOfWhen needs at least one available option whenever it runs.
Check the conditions passed to Checkr.Option, or provide a fallback option whose condition evaluates to true.");
