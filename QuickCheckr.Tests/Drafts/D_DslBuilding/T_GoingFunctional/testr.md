### Testr Parsing Numbers

**The Testr:**  
```csharp
Testr
    .Named("Parser matches golden model.")
    .For(ParseFuzzr.Expression(),
        Shrink.OnType<string>(
            s => s.Simplify(Reduce.Function<string>(ParseReducer.Reducer, true))))
    .DisableValueReduction()
    .Deliberate(a => a.Length, 4)
    .Assert(a => LostIn.Translation(a).Eval() == LostIn.FaultyTranslation(a).Eval());
```
`.Run()` is called without arguments, so it performs the default 100.Runs().  

**The Report:**  
```text
------------------------------------------------------------
  Parser matches golden model.
  Seed: 43650243
 ------------------------------------------------------------
  Falsified:
    Input = "- (2 - 1 / 3 + 2) ^ (1 / 1 * 3 - 1 / 3 / 1 + 1 * 3 / 2 / 1) / (2 - 1 / 1) / (1 + 1 / 2 * 3 - 2 / 1 * 1 + 1 * 3)"
    Redux = "-2^2"

  Original:
    "- (2 - 1 / 3 + 2) ^ (1 / 1 * 3 - 1 / 3 / 1 + 1 * 3 / 2 / 1) / (2 - 1 / 1) / (1 + 1 / 2 * 3 - 2 / 1 * 1 + 1 * 3)"
 ------------------------------------------------------------
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

**The Reducer:**  
```csharp
public static class ParseReducer
{
    public static IEnumerable<string> Reducer(string expr)
    {
        foreach (var item in RemoveOperations(expr))
            yield return item;
        string output = Regex.Replace(expr, @"\([^()]*\)", "2");
        yield return output;
        foreach (var item in RemoveOperations(output))
            yield return item;
    }
    private static IEnumerable<string> RemoveOperations(string expr)
    {
        var tokens = Lexer.Tokenize(expr);
        tokens = tokens.Where(a => a.Kind != TokenKind.LParen && a.Kind != TokenKind.RParen);
        yield return TokensToString(tokens);
        var list = tokens.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            yield return TokensToString(tokens.Skip(i));
            if (list[i].IsOperator && !list[i + 1].IsOperator)
                yield return TokensToString(tokens.Skip(i + 1));
        }
        for (int i = list.Count - 1; i > 0; i--)
        {
            if (list[i].IsOperator && !list[i - 1].IsOperator)
                yield return TokensToString(tokens.Take(i));
        }
    }
    private static string TokensToString(IEnumerable<Token> tokens)
        => string.Join("", tokens.Select(a => a.Text));
}
```
## Testr Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickTestr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Testr:**  
```csharp
Testr.Named("The sum of all values is less than 5 * 256.")
    .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
    .Deliberate(H.ElementCount, 2)
    .Assert(a => H.Sum(a) < (5 * 256));
```

**The Short List Fuzzr:**  
```csharp
Fuzzr.Short(short.MinValue, 256)
    .Many(0, 10)
    .Where(a => H.Sum(a) < 256)
    .ToList()
```

**The Helpers:**  
```csharp
public class H
{
    public static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;
    public static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));
    public static short Sum(IEnumerable<short> list)
    {
        short sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of all values is less than 5 * 256.
  Seed: 472166887
 ------------------------------------------------------------
  Falsified:
    Input = ( _, _, _, [ -23457 ], [ -25242 ] )
    Redux = ( _, _, _, [ -7527 ], [ -25242 ] )

  Original:
    ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
 ------------------------------------------------------------
```
### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Testr:**  
```csharp
Testr.Named("No more than four distinct integers.")
    .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
    .Deliberate(a => 0 - a.Count, -5)
    .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4);
```

**The Report:**  
```text
------------------------------------------------------------
  No more than four distinct integers.
  Seed: 1575924946
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
    Redux = [ [ _, 0 ], [ 1 ], [ 2 ], [ 3 ] ]

  Original:
    [ [ 91, 42, 27, 37, 85, 85, 40, 91, 75 ], [ 43, 58, 67 ], [ 78, 1, 82, 71, 59, 69, 81, 5 ], [ 56, 7, 12, 35 ], [ 74, 37, 40, 98, 14, 28 ], [ 26, 71, 93, 9, 98, 27 ], [ ], [ 78, 39, 94, 60 ], [ 52, 60 ], [ 30, 10, 9, 9, 87, 99 ], [ 30, 74, 50, 82, 18, 38, 93 ], [ 45, 40, 18, 52 ], [ 89, 62, 60 ], [ 79, 61, 63, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
 ------------------------------------------------------------
```
### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Testr:**  
```csharp
Testr
    .Named("Reversing a list of integers results in the same list")
    .For(Fuzzr.Int().Many(0, 10).ToList())
    .Assert(a =>
    {
        var reversed = new List<int>(a);
        reversed.Reverse();
        return reversed.SequenceEqual(a);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Reversing a list of integers results in the same list
  Seed: 12901993
 ------------------------------------------------------------
  Falsified:
    Input = [ _, 94 ]
    Redux = [ _, 0 ]

  Original:
    [ 76, 92, 75, 6, 94 ]
 ------------------------------------------------------------
```
### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Testr:**  
```csharp
Testr
    .Named("No division by zero")
    .For(TheFuzzr, TheShrinkr)
    .Format(TheFormatr)
    .Deliberate(a => DepthOf(a))
    .Assert(a => { a.Evaluate(); return true; });
```

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Shrinkr:**  
```csharp
Shrink.When<int>(s => s
    .As<Num>(a => a.Value)
    .As<Term, Expr>(a => a.R)
    .Value(t => t.Op == Operation.Div),
s => s.Simplify(Reduce.Towards(1)))
```

**The Formatter:**  
```csharp
[ Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
, Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())]
```

**The Helper:**  
```csharp
private static int DepthOf(Expr a, int depth = 0)
{
    if (a is Term term)
        return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
    return depth;
}
```

**The Report:**  
```text
------------------------------------------------------------
  DivideByZeroException: Attempted to divide by zero.
  Seed: 729093046
 ------------------------------------------------------------
  Falsified:
    Input = ( _ / ( 2 / _ ) )
    Redux = ( _ / ( 0 / _ ) )

  Original:
    { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 2 }, R: { Value: 3 } } }
 ------------------------------------------------------------
```
### Length List

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
### Difference

**The Fuzzr:**  
```csharp
from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
from pair in Fuzzr.One<Pair>()
select pair
```

**The Shrinkr:**  
```csharp
public class The
{
    public static readonly Shrinker Shrinkr =
        Shrink.OnType<Pair>(
            s => s.Simplify(
                Reduce.Function<Pair>(a => TowardsZero(a))));
    private static IEnumerable<Pair> TowardsZero(Pair input)
    {
        var local = input;
        while (local.A >= 0)
        {
            local = local with { A = local.A - 1, B = local.B - 1 };
            yield return local;
        }
    }
}
```
#### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be zero.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || a.A != a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be zero.
  Seed: 1485535450
 ------------------------------------------------------------
  Falsified:
    Input = { A: 93, B: 93 }
    Redux = { A: 10, B: 10 }

  Original:
    { A: 93, B: 93 }
 ------------------------------------------------------------
```
#### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be small.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be small.
  Seed: 1449083695
 ------------------------------------------------------------
  Falsified:
    Input = { A: 37, B: 38 }
    Redux = { A: 10, B: 6 }

  Original:
    { A: 37, B: 38 }
 ------------------------------------------------------------
```
#### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference is not exactly 1.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference is not exactly 1.
  Seed: 1231462692
 ------------------------------------------------------------
  Falsified:
    Input = { A: 51, B: 52 }
    Redux = { A: 10, B: 9 }

  Original:
    { A: 51, B: 52 }
 ------------------------------------------------------------
```
### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Testr:**  
```csharp
Testr.Named("Heap sort is correct.")
    .For(TheFuzzr)
    .Deliberate(a => Flatten(a).Count())
    .Assert(a =>
    {
        var correct = Flatten(a).OrderBy(x => x).ToList();
        var buggy = Flatten(a).ToList();
        return correct.SequenceEqual(buggy);
    });
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Report:**  
```text
------------------------------------------------------------
  Heap sort is correct.
  Seed: 2136249593
 ------------------------------------------------------------
  Falsified:
    Input = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
    Redux = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 0, Left: { }, Right: { } } }

  Original:
    { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ------------------------------------------------------------
```
### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Testr:**  
```csharp
Testr
    .Named("No two different indexes point to each other.")
    .For(
        from length in Fuzzr.Int()
        from enumerable in Fuzzr.Int(0, length - 1).Many(length)
        select enumerable.ToList())
    .DisableValueReduction()
    .Deliberate(a => a.Count, 2)
    .Assert(list => list.All(
        element =>
        {
            int index = list[element];
            if (index != element && list[index] == element)
                return false;
            return true;
        }));
```

**The Report:**  
```text
------------------------------------------------------------
  No two different indexes point to each other.
  Seed: 1934478623
 ------------------------------------------------------------
  Falsified:
    Input = [ 1, 0 ]


  Original:
    [ 1, 1, 0, 4, 3, 3, 2, 3, 11, 6, 4, 4, 4, 12 ]
 ------------------------------------------------------------
```
### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Testr:**  
```csharp
Testr
    .Named("Element is no longer in the list.")
    .For(Fuzzr.Tuple(Fuzzr.Int(1, 10).Many(3, 20), Fuzzr.Int(1, 10)))
    .Assert(a =>
    {
        var list = a.Item1.ToList();
        list.Remove(a.Item2);
        return !list.Contains(a.Item2);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Element is no longer in the list.
  Seed: 712389878
 ------------------------------------------------------------
  Falsified:
    Input = ( [ 1, 1 ], 1 )


  Original:
    ( [ 1, 1, 8, 3, 5, 1, 9, 1, 7, 2, 8, 6, 3 ], 1 )
 ------------------------------------------------------------
```
### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Testr:**  
```csharp
Testr
    .Named("Contains at least three distinct elements.")
    .For(Fuzzr.Int(1, 10).Many(3, 20))
    .Assert(a =>
    {
        if (a.Count() >= 3)
            return a.ToHashSet().Count >= 3;
        return true;
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Contains at least three distinct elements.
  Seed: 1943621438
 ------------------------------------------------------------
  Falsified:
    Input = [ 3, _, 3 ]


  Original:
    [ 3, 9, 3, 9, 3 ]
 ------------------------------------------------------------
```
### Nested Lists

**The Testr:**  
```csharp
Testr
    .Named("The sum of lengths of the element lists is at most 10.")
    .For(Fuzzr.Int().Many(0, 20).ToList().Many(0, 20).ToList())
    .Deliberate(a => a.Count)
    .Assert(a => a.Sum(a => a.Count) <= 10);
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of lengths of the element lists is at most 10.
  Seed: 1959968277
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]


  Original:
    [ [ 11, 46, 66 ], [ 48, 3, 13, 76, 75, 42, 97, 16, 59, 9, 29, 78, 17, 52, 68, 23, 94, 22, 8, 34 ], [ 87, 40, 45, 37, 74, 94, 74, 84, 54, 40, 38, 27, 59 ], [ 2, 23, 80, 49 ], [ 93, 39, 12, 70, 49, 30 ], [ 21, 84, 41, 23, 89, 38, 32, 36, 61, 62, 68, 39, 2, 47, 23 ], [ 6, 69, 91, 51, 58, 30 ], [ 1, 14, 72, 76, 41, 25, 75, 70, 96, 7, 99, 74, 89, 44 ], [ 50, 99, 94, 23, 61, 39 ], [ 41, 48, 84, 70, 8, 7, 1, 57, 78, 17, 90 ], [ 41, 10, 18 ], [ 91, 12, 85, 92, 72, 32, 2, 73, 33, 87, 74, 20 ], [ 56, 35, 69, 28, 45, 72, 24, 92, 70, 17, 69, 69, 39, 29, 83, 84, 72, 4, 22 ] ]
 ------------------------------------------------------------
```
