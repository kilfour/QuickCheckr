# Test Framework Integration
[The Board][PuttingACaseOnTheBoard] | [Next][RunningACheckr]  
  
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[RunningACheckr]: ../B_RunningACheckr/RunningACheckr.md
