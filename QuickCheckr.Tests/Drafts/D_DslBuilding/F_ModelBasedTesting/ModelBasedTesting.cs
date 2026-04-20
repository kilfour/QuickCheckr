using QuickCheckr.Authoring;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.F_ModelBasedTesting;

[DocFile]
[DocModelHeader]
[DocExample(typeof(CalculatorModel))]
[DocBoldHeader("SUT")]
[DocExample(typeof(Calculator))]
public class ModelBasedTesting : QCTest;

[CodeExample]
public class CalculatorModel
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}

[CodeExample]
public class Calculator
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }

    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}