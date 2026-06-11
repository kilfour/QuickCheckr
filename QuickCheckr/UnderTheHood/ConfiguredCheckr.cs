using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood;

public record ConfiguredCheckr(CheckrOf<Case> Checkr, CheckrConfig Config);
