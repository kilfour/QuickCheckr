using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class NoCheckrsProvidedException()
    : QuickCheckrException(
        @"Checkr.OneOf could not choose a Checkr because no options were provided.

OneOf needs at least one Checkr to choose from.
Pass one or more Checkrs to OneOf.");
