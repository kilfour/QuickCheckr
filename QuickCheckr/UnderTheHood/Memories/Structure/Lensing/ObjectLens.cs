using System.Reflection;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

public class ObjectLens(PropertyInfo propertyInfo)
    : MemoryLens(
        obj => propertyInfo.GetValue(obj)!,
        (val, propValue) => { SetPropertyValue(propertyInfo, val!, propValue); return val; }
    )
{
    private static void SetPropertyValue(PropertyInfo propertyInfo, object target, object value)
    {
        var prop = propertyInfo;
        if (!prop.CanWrite)
            prop = propertyInfo.DeclaringType!.GetProperty(propertyInfo.Name);

        if (prop != null && prop.CanWrite)
            prop.SetValue(target, value, null);
    }
}