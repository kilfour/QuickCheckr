using QuickCheckr.UnderTheHood.Memories;

namespace QuickCheckr.Tests.Verification;

public class SwapperTests
{
    [Fact]
    public void ScopedSwap()
    {
        var memory = new Memory();
        memory.StartNewExecution();
        memory.StoreInput("int", () => 0);
        Assert.Equal(0, Get(memory, 1, "int"));
        using (memory.ScopedSwap(1, "int", 42))
        {
            Assert.Equal(42, Get(memory, 1, "int"));
        }
        Assert.Equal(0, Get(memory, 1, "int"));
    }

    private int Get(Memory memory, int executionId, string key) =>
        (int)memory.ShortTermMemory.GetMemory(executionId).GetInputs()[key]!;

    // [Fact]
    // public void ScopedSwap_list()
    // {
    //     var memory = new Memory(() => 0);
    //     List<int> list = [1];
    //     memory.AccessForForThisExecution().SetIfNotAlreadyThere("list", list);
    //     var swapper = new MemoryLens(l => ((List<int>)l)[0], (l, el) => { list[0] = (int)el; return el; });
    //     using (memory.NestedValue(swapper))
    //     {
    //         using (memory.ScopedSwap("list", 42))
    //         {
    //             Assert.Equal(42, memory.AccessForForThisExecution().Get<int>("list"));
    //         }
    //     }
    // }

    // [Fact]
    // public void ScopedSwap_list_in_list()
    // {
    //     var memory = new Memory(() => 0);
    //     List<int> list = [1];
    //     List<List<int>> outer = [list];
    //     memory.AccessForForThisExecution().SetIfNotAlreadyThere("list", outer);

    //     var outerSwapper = new MemoryLens(
    //         l => ((List<List<int>>)l)[0],
    //         (l, el) => { ((List<List<int>>)l)[0] = (List<int>)el; return l; });

    //     var swapper = new MemoryLens(
    //         l => ((List<int>)l)[0],
    //         (l, el) => { ((List<int>)l)[0] = (int)el; return l; });

    //     using (memory.NestedValue(outerSwapper))
    //     {
    //         using (memory.NestedValue(swapper))
    //         {
    //             using (memory.ScopedSwap("list", 42))
    //             {
    //                 Assert.Equal(42, list.First());
    //             }
    //         }
    //     }
    // }
}