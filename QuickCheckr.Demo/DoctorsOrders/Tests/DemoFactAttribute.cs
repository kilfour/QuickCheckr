using System.Runtime.CompilerServices;
using Xunit;

namespace QuickCheckr.Demo.DoctorsOrders.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DemoFactAttribute : FactAttribute
{
    public const string EnvironmentFlag = "QUICKCHECKR_DEMO_TESTS";

    public DemoFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0) : base(sourceFilePath, sourceLineNumber)
    {
        var enabled = Environment.GetEnvironmentVariable(EnvironmentFlag);
        if (enabled != "1" && !string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
            Skip = $"Set {EnvironmentFlag}=1 to run demo tests.";
    }
}

