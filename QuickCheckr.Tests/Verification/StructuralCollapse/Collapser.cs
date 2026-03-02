using System.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification.StructuralCollapse;

public class Collapser
{
    public static object? Collapse(Func<bool> stillFails, ShrinkingInfo info)
    {
        if (typeof(IEnumerable).IsAssignableFrom(info.Type))
        {
            var theList = info.Value! as IList;
            int index = 0;
            while (index < theList?.Count)
            {
                var ix = index;
                var before = theList[ix];
                theList.RemoveAt(ix);
                if (stillFails()) { continue; }
                theList.Insert(ix, before);
                index++;
            }
        }
        return info.Value;
    }
}