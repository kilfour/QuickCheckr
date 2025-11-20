using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;


namespace QuickCheckr.UnderTheHood;

public class FalsifiableException : Exception
{
	public CaseFile CaseFile { get; private set; }

	public FalsifiableException(CaseFile caseFile)
		: base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile)))
	{
		CaseFile = caseFile;
	}
	public FalsifiableException(CaseFile caseFile, Exception? exception)
		: base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile)), exception)
	{
		CaseFile = caseFile;
	}

	private static CaseFile AddTestMethodInfo(CaseFile caseFile)
	{
		var assembly = typeof(Case).Assembly;
		var trace = new StackTrace(fNeedFileInfo: true);
		foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
		{
			var file = frame.GetFileName();
			if (!string.IsNullOrEmpty(file))
			{
				var method = frame.GetMethod();
				if (method == null) continue;
				var declaringAssembly = method.DeclaringType?.Assembly;
				if (declaringAssembly == assembly) continue;
				caseFile.Verdict!.AddTestMethodDisposition(
					new TestMethodInfoDeposition(
						method?.Name ?? "unknown_test", file, frame.GetFileLineNumber()));
				return caseFile;
			}
		}
		return caseFile;
	}
}