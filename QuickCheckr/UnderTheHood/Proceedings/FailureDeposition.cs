namespace QuickCheckr.UnderTheHood.Proceedings
{
    public abstract class FailureDeposition { public abstract string GetFailureDescription(); };

    public class ExceptionDeposition : FailureDeposition
    {
        public Exception Exception { get; }

        public ExceptionDeposition(Exception exception)
        {
            Exception = exception;
        }

        public override string GetFailureDescription()
        {
            return Exception.Message;
        }
    };

    public class FailedSpecDeposition : FailureDeposition
    {
        public string FailedSpec { get; }

        public FailedSpecDeposition(string failedSpec)
        {
            FailedSpec = failedSpec;
        }

        public override string GetFailureDescription()
        {
            return FailedSpec;
        }
    };

    public class AssayerDeposition : FailureDeposition
    {
        public string FailedSpec { get; }

        public AssayerDeposition(string failedSpec)
        {
            FailedSpec = failedSpec;
        }

        public override string GetFailureDescription()
        {
            return FailedSpec;
        }
    };
}