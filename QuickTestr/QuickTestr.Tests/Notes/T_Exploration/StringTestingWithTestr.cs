using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Show;
using QuickTestr.Bolts;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Notes.T_Exploration;

public class StringTestingWithTestr : QuickTestrPropertyTest<StringTestingWithTestr>
{
    protected override bool Asserts => true;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact(Skip = "experiment")]
    public override void Example() => Document();

    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Create Book Title")
            .For(Text(100))
            .Assert(a => BookTitle.Create(a!).Value == a)
            .StoreCaseFiles(journalist)
            .FillVault(200.Searches(), 50.Runs(),
                new VaultPolicy<string?>(a => a?.Length <= 1 ? a : a?.Length!));

    protected override void Verify(Article article)
    {

    }

    public static FuzzrOf<string?> Text(int maxLength)
    {
        FuzzrOf<string> whitespace =
            Fuzzr.OneOf(" ", "\t", "\r", "\n", "\r\n", "\u00A0");

        FuzzrOf<string> padded =
            from value in Fuzzr.String(1, maxLength)
            from left in whitespace
            from right in whitespace
            select $"{left}{value}{right}";

        FuzzrOf<string> boundaries =
            Fuzzr.OneOf(
                Fuzzr.String(maxLength - 1),
                Fuzzr.String(maxLength),
                Fuzzr.String(maxLength + 1));

        return Fuzzr.OneOf(
            (1, Fuzzr.Constant<string?>(null)),
            (1, Fuzzr.Constant<string?>("")),
            (2, whitespace),
            (5, Fuzzr.String(1, maxLength)),
            (3, padded),
            (4, boundaries),
            (2, Fuzzr.String(maxLength + 2, maxLength * 2)));
    }

    public static FuzzrOf<string> AlphaNumeric(int min = 1, int max = 10) =>
        from firstChar in Fuzzr.Char()
        from rest in Fuzzr.OneOf(Fuzzr.Char(), Fuzzr.Char('0', '9')).Many(1, 10)
        select new string([.. rest.Prepend(firstChar)]);

    [Fact]
    public void Foo()
    {
        AlphaNumeric(3, 20).Many(20).Generate().PulseToQuickLog();
    }
}
