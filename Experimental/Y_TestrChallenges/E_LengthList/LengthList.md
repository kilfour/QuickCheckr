# Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Testr:**  
```csharp
Testr.Named("The maximum value of the list is smaller than 900.")
    .For(
        from length in Fuzzr.Int(1, 100)
        from list in Fuzzr.Int(0, 1000).Many(length)
        select list.ToList())
    .Assert(a => a.Max() < 900);
```

**The Report:**  
```text
------------------------------------------------------------
  The maximum value of the list is smaller than 900.
  Seed: 357470573
 ------------------------------------------------------------
  Falsified:
    Input = [ 905 ]
    Redux = [ 900 ]

  Original:
    [ 176, 244, 257, 301, 257, 329, 381, 764, 442, 261, 535, 550, 677, 905 ]
 ------------------------------------------------------------
```
