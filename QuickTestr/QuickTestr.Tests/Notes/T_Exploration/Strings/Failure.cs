using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse;

namespace QuickTestr.Bolts.Builders.Strings;


public interface IFailure<TResult>
{
    public CheckrOf<Case> GetCheckr(Func<string, TResult> factory);
}

public record Failure<TException, TResult>(string Message, StringCase[] Cases) : IFailure<TResult> where TException : Exception
{
    public CheckrOf<Case> GetCheckr(Func<string, TResult> factory) =>
        Combine.Checkrs(
            Cases.Select(stringCase =>
                from result in stringCase.GetActCheckr(factory)
                from threw in Checkr.ExpectThrewExactly<TException>($"{stringCase.Label}: Threw '{typeof(TException).Name}'", result)
                from checkMessage in Checkr.Expect($"{stringCase.Label}: Message Equals '{Message}'", () => Message.Equals(result.Exception?.Message))
                select Case.Closed
            ));

}
