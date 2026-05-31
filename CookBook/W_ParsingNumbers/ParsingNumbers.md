# Parsing Numbers
Another non-stateful testing example. And one where a Reducer is absolutely necessary.

I used this one, during a course I was teaching to verify student code,
because I did not feel like combing through all the code looking for bugs manually. 

  
**Demonstrates:**
- Oracle style testing
- Custom Reducers
- Custom Deliberation Policies
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Expression",
    ParseFuzzr.Expression(),
    Shrink.OnType<string>(s => s.Simplify(
        // remove whitespace
        Stringr.AtleastOnce(char.IsWhiteSpace).Remove().Defined(),
        // remove delimiters
        Stringr.Balanced('(', ')').Replace(t => t[1..^1]).Defined(),
        // remove lhs and op
        from lhs in Stringr.AtleastOnce(char.IsDigit).Remove()
        from op in Stringr.Once(operators).Remove()
        select Selection.Defined,
        // remove op and rhs
        from op in Stringr.Once(operators).Remove()
        from rhs in Stringr.AtleastOnce(char.IsDigit).Remove()
        select Selection.Defined,
        // clean up unary minus noise
        Stringr.AtleastOnce('-').Replace(t => "-").Defined())))
from oracleResult in Checkr.Act("Oracle Result", () => LostIn.Translation(input).Eval())
from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
from traceResult in Checkr.Trace("Compare", () => $"Oracle: {oracleResult} /= SUT: {result}")
from match in Checkr.ExpectWhen("Match",
    () => !double.IsNaN(oracleResult),
    () => oracleResult == result)
select Case.Closed;
```

**The Configuration:**  
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with
    {
        Deliberation = new Deliberation(cf => cf.InputsNamed<string>("Expression", a => a.Length), 4),
        ReportMode = config.ReportMode & ~ReportMode.Labels
    };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ParsingNumbers.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    640099888
 ------------------------------------------------------------
  Executed: Oracle Result, SUT Result
   - Expression = "1 ^ (3 * 2 + 2 - 3 / 3 * 2) - (1 / 1 + 1 * 1 + 1 * 1 / 2 * 2 + 3 / 3) ^ 1 / - 1 ^ 2"
     Redux        "-1^2"
   - Compare    = Oracle: 5 /= SUT: -3
 ================================
  !! Expectation Failed: Match
 ================================
```

**The Fuzzr:**  
```csharp
public class ParseFuzzr
{
    public static FuzzrOf<string> Expression(int maxDepth = 4) => Expr(maxDepth);
    public static readonly FuzzrOf<string> Number = from i in Fuzzr.Int(1, 4) select i.ToString();
    private static readonly FuzzrOf<string> AddOp = Fuzzr.OneOf("+", "-");
    private static readonly FuzzrOf<string> MulOp = Fuzzr.OneOf("*", "/");
    private static readonly FuzzrOf<string> PowOp = Fuzzr.Constant("^");
    private static readonly FuzzrOf<string> WS =
        from chs in Fuzzr.Constant(' ').Many(1)
        select new string([.. chs]);
    private static readonly FuzzrOf<string> UnaryOp = Fuzzr.OneOf("-");
    private static FuzzrOf<string> Primary(int maxDepth) =>
        maxDepth <= 0
        ? Number
        : Fuzzr.OneOf(
            Number,
            from l in Fuzzr.Constant("(")
            from e in Expr(maxDepth - 1)
            from r in Fuzzr.Constant(")")
            select l + e + r,
            from op in UnaryOp
            from ws in WS
            from inner in Primary(maxDepth - 1)
            select op + ws + inner
          );
    private static FuzzrOf<string> Factor(int maxDepth) =>
        from a in Primary(maxDepth)
        from tail in maxDepth <= 0
            ? Fuzzr.Constant("")
            : Fuzzr.OneOf(
                  Fuzzr.Constant(""),
                  from w1 in WS
                  from op in PowOp
                  from w2 in WS
                  from b in Factor(maxDepth - 1)
                  select w1 + op + w2 + b)
        select a + tail;
    private static FuzzrOf<string> Term(int maxDepth) =>
        from head in Factor(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Factor(maxDepth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
    private static FuzzrOf<string> Expr(int maxDepth) =>
        from depth in Fuzzr.Int(0, maxDepth)
        from head in Term(depth)
        from pieces in (
            from w1 in WS
            from op in AddOp
            from w2 in WS
            from rhs in Term(depth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
}
```
This Fuzzr generates a valid numeric expression string of variable length.  
 *Some Examples:*  
```text
(2 * 1 / 1 / 3 + 3) ^ - 1 ^ 1 / - 2 / 3 ^ 3 * - 1
- 1 ^ 3 / 2 / 1 * 1
- 3
- 2 * 1 * 2 * 3 - 1 * 1 * 1 / 3 + 2
(1 - 1 - 1 / 2) ^ - 2 ^ 3 / (1 / 2 * 3 / 2 - 1 * 2 / 1) ^ 2 - - 2 ^ 2 + - 3 ^ 3
```
