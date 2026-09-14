using QuickFuzzr;
using QuickTestr;

var observed = 0;

Testr.Named("package smoke test")
    .For(Fuzzr.Constant(42))
    .Assert(value =>
    {
        observed = value;
        return value == 42;
    })
    .Run();

if (observed != 42)
    throw new InvalidOperationException("The packaged QuickTestr did not execute the test.");

Console.WriteLine("QuickTestr package smoke test passed.");
