using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse;

namespace QuickTestr.Bolts.Builders.Strings;

public class StringTestrExplore<TResult>(string name, Func<string, TResult> factory)
{
    private bool storeCaseFiles = false;
    protected ICustodian? custodian;
    private bool addLengthExploration = true;

    private FuzzrOf<string>? fuzzr = null;

    public StringTestrExplore<TResult> Accepts(FuzzrOf<string> fuzzr)
    {
        this.fuzzr = fuzzr;
        return this;
    }
    public StringTestrExplore<TResult> StoreCaseFiles(ICustodian? custodian = null)
    {
        storeCaseFiles = true;
        this.custodian = custodian;
        return this;
    }

    private readonly List<StringCase> casesToCheck =
        [ StringCase.Null
        , StringCase.Empty
        , StringCase.Whitespace
        ];

    private readonly List<IFailure<TResult>> failures = [];

    public StringTestrExplore<TResult> FailsWith<TException>(string message, params StringCase[] cases)
        where TException : Exception
    {
        foreach (var stringCase in cases)
        {
            casesToCheck.Remove(stringCase);
        }
        if (cases.Any(a => a is StringCaseMaxLength))
            addLengthExploration = false;
        failures.Add(new Failure<TException, TResult>(message, cases));
        return this;
    }

    public ConfiguredCheckr Explore() => (
        from collector in Trackr.Stashed(() => new List<string>())
        from once in Trackr.Once("Hardcoded values",
           from checks in Combine.Checkrs(
               casesToCheck.Select(stringCase => stringCase.GetCheckr(factory)))
           from failureChecks in Combine.Checkrs(
               failures.Select(failure => failure.GetCheckr(factory)))
           select Case.Closed)
        from maybeAccepts in Checkr.When(() => fuzzr != null,
           from input in Checkr.Input("Input", fuzzr!)
           from act in Checkr.ActCarefully("Accepts", () => factory(input))
           from report in Checkr.When(() => act.Threw,
               Checkr.Perform(() => collector.AddRange(StringCase.GetMessage("Accepts", act))))
           from collect in Checkr.When(() => !act.Threw, Checkr.Expect("Accepts", () => true))
           select Case.Closed)
        from maybeLength in Checkr.When(() => addLengthExploration,
           from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"), Shrink.None<int>())
           from length in Checkr.Input("Length", Fuzzr.Constant(counter * 20),
               Shrink.OnType<int>(s => s.Classify(Try.Keep<int>()), s => s.Simplify(Reduce.Towards(0))))
           from longString in Checkr.ActCarefully("Length", () => factory(new string('a', length)))
           from expect in Checkr.Expect("Long String", () => !longString.Threw, () => [.. collector.Concat(StringCase.GetMessage(length.ToString(), longString))])
           select length)
        from verify in Trackr.Verify("Results", () => collector.Count == 0,
               () =>
               {
                   if (addLengthExploration)
                       collector.Add($"Long String {maybeLength.Value}: Success");
                   return collector;
               })
        select Case.Closed)
        .Configure(a => a with
        {
            FileAs = storeCaseFiles ? name : string.Empty,
            Custodian = custodian is null ? Custodian.Default : custodian,
            ShrinkMode = a.ShrinkMode | ShrinkMode.Reduction,
            Clerk = new StringTestrClerk()
        })
        .Run(GetExecutionCount());

    private ExecutionCount GetExecutionCount()
    {

        return addLengthExploration || fuzzr != null ? 50.ExecutionsPerRun() : 10.ExecutionsPerRun();
    }
}
