using System.Collections;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

internal sealed class ListLens(Type elementType, int ix)
    : MemoryLens(
        list => ((IList)list)[ix]!,
        (list, element) =>
        {
            if (element == null || elementType.IsAssignableFrom(element.GetType()))
                ((IList)list)[ix] = element;
            else
                throw new InvalidCastException();
            return list;
        });
