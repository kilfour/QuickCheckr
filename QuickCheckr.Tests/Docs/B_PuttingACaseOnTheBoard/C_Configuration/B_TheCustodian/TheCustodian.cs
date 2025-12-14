using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

[DocFile]
[DocContent(
@"`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
")]
[DocHeader("Custom Custodians")]
[DocExperimental]
[DocContent("You can also supply your own implementation:")]
[DocCodeFile("InMemoryCustodian.cs", "csharp", 5)]
[DocContent("This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem")]
public class TheCustodian;