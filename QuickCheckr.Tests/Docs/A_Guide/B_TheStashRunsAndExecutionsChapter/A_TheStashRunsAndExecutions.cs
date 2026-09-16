using QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;
using QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMoveChapter;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutionsChapter;

[DocFile]
[DocLink(typeof(InPoolsWeTrust), "InPoolsWeTrust")]
[DocFileHeader("The Stash, Runs and Executions")]
[DocNavigation(typeof(Guide), typeof(A_CheckringThingsOut), typeof(A_ChoosingTheNextMove))]
[DocContent(
@"In the previous chapter we upped the number of runs in order to explore more of the input space.  
But what if it's not the input that causes the issue, but a side-effect ?

Consider the following model:")]
[DocExample(typeof(BugHouse))]
[DocInclude(typeof(B_DelayedDetonationNotFound), true)]
[DocInclude(typeof(C_DelayedDetonationFound), true)]
public class A_TheStashRunsAndExecutions
{
    [CodeExample]
    public class BugHouse
    {
        private int count;
        public bool DelayedDetonation(int a)
        {
            if (count++ == 2 && a == 1)
                throw new Exception("I'm sorry, Dave, I'm afraid I can't do that.");
            return true;
        }
    }
}