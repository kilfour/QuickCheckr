using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public class DocExplanationInProgress() :
    DocContentAttribute(
@"
!! WARNING: **Unfinished Documentation** !!

> This feature is finished, the tests are there.  
>  
> But ..., we're working on writing down the explanation.  
> Depending on how far we are in this process, you could just see some scaffolding files here.  
> Or the raw output from the Unit Tests.  
> Or an unfinished, unpolished version of the docs.
>
> Come back soon.
");
