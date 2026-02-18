# Magical Shrinking 
```csharp
using FsCheck;

public static class Demo
{
    public static void MagicalShrinkExample()
    {
        // Domain is explicitly 1..100 (0 is impossible to generate)
        var gen = Gen.Choose(1, 100);

        // Property: x must be > 10
        // This fails for x in 1..10.
        // BUT it also fails for x = 0.
        Prop.ForAll(gen, x => x > 10)
            .QuickCheckThrowOnFailure();
    }
}
```

Explanation is sometimes better produced by variation than by minimization. 

# Example

## FsCheck
```csharp
Prop.ForAll(
    from r in Arb.Generate<NonNegativeDouble>().Select(x => x.Item)
    from a in Arb.Generate<double>()
    select new { Radius = r, Angle = a },
    input =>
    {
        var cartesian = ComplexMath.ToCartesian(input.Radius, input.Angle);
        var polar = ComplexMath.ToPolar(cartesian.X, cartesian.Y);
        
        return Math.Abs(polar.Radius - input.Radius) < 1e-10 
            && Math.Abs(polar.Angle - input.Angle) < 1e-10;
    })
.QuickCheck();
```

## QC Now
```csharp
(from radius in Checkr.Input("Radius", Fuzzr.Double(0, 1000), Reduce.TowardsZero)
from angle in Checkr.Input("Angle", Fuzzr.Double(-Math.PI, Math.PI), Reduce.TowardsZero)
from cartesian in Checkr.Capture(() => ComplexMath.ToCartesian(radius, angle))
from polar in Checkr.Capture(() => ComplexMath.ToPolar(cartesian.X, cartesian.Y))
from radiusOk in Checkr.Expect("Radius preserved", () => Math.Abs(polar.Radius - radius) < 1e-10)
from angleOk in Checkr.Expect("Angle preserved", () => Math.Abs(polar.Angle - angle) < 1e-10)
select Case.Closed).Run(100.ExecutionsPerRun());
```

## QC Goal
```csharp
Check.ForAll(
    from r in Fuzzr.Double(0, 1000)
    from a in Fuzzr.Double(-Math.PI, Math.PI)
    select new { Radius = r, Angle = a }, 
    input =>
    {
        var cartesian = ComplexMath.ToCartesian(input.Radius, input.Angle);
        var polar = ComplexMath.ToPolar(cartesian.X, cartesian.Y);
        return Math.Abs(polar.Radius - input.Radius) < 1e-10 
            && Math.Abs(polar.Angle - input.Angle) < 1e-10;
    })
```
or better with labels and splitting the predicate

```csharp
(from input in Checkr.Input("Input", 
    from r in Fuzzr.Double(0, 1000)
    from a in Fuzzr.Double(-Math.PI, Math.PI)
    select new { Radius = r, Angle = a })
from radiusOk in Checkr.Expect("Radius & Angle preserved", 
    () => Math.Abs(polar.Radius - input.Radius) < 1e-10 
        && Math.Abs(polar.Angle - input.Angle) < 1e-10))
.Run(100.ExecutionsPerRun())
```

# Another Example

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public static class Tags
{
    public static IReadOnlyList<string> Parse(string input)
        => input.Split(',')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .OrderBy(x => x)
            .ToList();

    public static string Render(IEnumerable<string> tags)
        => string.Join(", ", tags
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .OrderBy(x => x));
}

```

```csharp
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;

public class TagsProperties
{
    // 1) Render(Parse(s)) is idempotent (canonical form)
    [Property]
    public void RenderParse_is_idempotent(NonNull<string> s)
    {
        var input = s.Get;
        var once = Tags.Render(Tags.Parse(input));
        var twice = Tags.Render(Tags.Parse(once));
        // canonicalization should stabilize
        once.Should().Be(twice);
    }

    // 2) Parse(Render(tags)) == Parse(string.Join(",", tags)) (same semantics)
    [Property]
    public void ParseRender_preserves_semantics(NonEmptyArray<NonNull<string>> rawTags)
    {
        var tags = rawTags.Get.Select(x => x.Get).ToArray();
        var rendered = Tags.Render(tags);
        var parsed1 = Tags.Parse(rendered);
        var parsed2 = Tags.Parse(string.Join(",", tags));
        parsed1.Should().Equal(parsed2);
    }
}
```