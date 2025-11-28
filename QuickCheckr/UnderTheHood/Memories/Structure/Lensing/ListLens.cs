using System.Collections;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

public class ListLens(Type elementType, int ix)
    : MemoryLens(
        list => ((IList)list)[ix]!,
        (list, element) =>
        {
            if (element == null || elementType.IsAssignableFrom(element.GetType()))
                ((IList)list)[ix] = element;
            return list;
        });
