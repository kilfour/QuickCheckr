# Parsing Numbers
```csharp
    from input in Checkr.Input("Expression", ParseFuzzr.Expression(), a => a with { Reducer = ParseReducer.Reducer })
    from trace in Checkr.Trace("Original", () => input)
    from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
    from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
    from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
    from match in Checkr.SpecIf("Match",
        () => !double.IsNaN(goldenResult),
        () => goldenResult == result)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\Q_ParsingNumbers\ParsingNumbers.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    406406480
 ------------------------------------------------------------
  Executed (1): Golden Result, SUT Result
   - Input: Expression = "2^-2^2"
   - Trace: Original = (2 * 1 - 3 * 2 * 1 * 3 + 1 / 2 * 1) ^ - 2 ^ 2 / 3 ^ 2 / 3 - - 3 / 2
   - Trace: Compare = Golden: 1,5000006416666136 /= SUT: 2139,2800925925926
 =========================
  !! Spec Failed: Match
 =========================
```
