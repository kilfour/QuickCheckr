
using System.Collections;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal static class CloneList
{
    public static IList AsOriginalType(object value)
    {
        if (value is not IEnumerable enumerable)
            throw new ArgumentException("Value is not an IEnumerable", nameof(value));

        var valueType = value.GetType();
        Type elementType;

        if (valueType.IsArray)
        {
            elementType = valueType.GetElementType()!;
        }
        else if (valueType.IsGenericType)
        {
            elementType = valueType.GetGenericArguments().First();
        }
        else
        {
            elementType = typeof(object);
        }

        var typedListType = typeof(List<>).MakeGenericType(elementType);
        var typedList = (IList)Activator.CreateInstance(typedListType)!;

        foreach (var item in enumerable)
        {
            typedList.Add(item);
        }

        return typedList;
    }

    public static object GetSubstituteForType(Type type, IList theList)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var array = Array.CreateInstance(elementType, theList.Count);
            for (var i = 0; i < theList.Count; i++)
            {
                array.SetValue(theList[i], i);
            }
            return array;
        }
        return theList;
    }
}
