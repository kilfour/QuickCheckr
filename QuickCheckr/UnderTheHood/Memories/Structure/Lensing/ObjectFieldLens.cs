using System.Reflection;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

internal sealed class ObjectFieldLens(FieldInfo fieldInfo)
    : MemoryLens(
        obj => fieldInfo.GetValue(obj)!,
        (val, fieldValue) => { fieldInfo.SetValue(val, fieldValue); return val; }
    );
