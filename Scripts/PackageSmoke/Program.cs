using QuickCheckr;
using QuickFuzzr;

var observed = 0;
var checkr =
    from input in Checkr.Input("number", Fuzzr.Constant(42))
    from action in Checkr.Act("remember number", () => observed = input)
    from expectation in Checkr.Expect("number was remembered", () => observed == 42)
    select Case.Closed;

checkr.Run();

if (observed != 42)
    throw new InvalidOperationException("The packaged QuickCheckr did not execute the check.");

Console.WriteLine("QuickCheckr package smoke test passed.");
