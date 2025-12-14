using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public class DocExperimentalAttribute() :
    DocContentAttribute(
@"
!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

");