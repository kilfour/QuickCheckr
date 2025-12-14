 ------------------------------------------------------------
 Test:                    Example
 Location:                CreateReadMe.cs:26:1
 Original failing run:    85 executions
 Minimal failing case:    5 executions (after 82 shrinks)
 Seed:                    1141724745
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: number = 6
 ------------------------------------------------------------
  Executed: BugHouse.Run (2 Times)
 ===========================================================================
  !! Exception Thrown: System.Exception: Here's Johnny
   at QuickCheckr.Tests.CreateReadMe.BugHouse.Run(Int32 number) in CreateReadMe.cs:line 69
   at QuickCheckr.Tests.CreateReadMe.<>c__DisplayClass2_0.GetCheckr() in CreateReadMe.cs:line 57
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
 Passed Expectations
 - Returns true: 84x
 ------------------------------------------------------------
