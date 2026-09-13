using System.Reflection;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

internal sealed class ObjectTuplePropertyLens(PropertyInfo propertyInfo)
    : MemoryLens(
        obj => propertyInfo.GetValue(obj)!,
        (val, propValue) => { SetPropertyValue(propertyInfo, val!, propValue); return val; }
    )
{
    private static void SetPropertyValue(PropertyInfo propertyInfo, object target, object value)
    {
        if (int.TryParse(propertyInfo.Name["Item".Length..], out var index))
        {
            SetTupleItem(target, index, value);
        }
    }

    public static void SetTupleItem(object tuple, int itemIndex, object? value)
    {
        var type = tuple.GetType();

        // System.Tuple<T1,T2> stores fields as m_Item1, m_Item2, ...
        var field = type.GetField(
            $"m_Item{itemIndex}",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
            throw new InvalidOperationException(
                $"Could not find backing field m_Item{itemIndex} on {type.FullName}");

        field.SetValue(tuple, value);
    }
}
