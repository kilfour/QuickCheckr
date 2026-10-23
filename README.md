# <img src='icon.png' width='40' align='top'/> QuickCheckr
> **CSI: .NET**

Property-based testing for the pragmatic.

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)

**QuickCheckr** is a C# library for exploring how your code behaves with generated
inputs and sequences of operations. You describe what the system can do and what
should remain true. QuickCheckr runs those descriptions, looks for failures, and
shrinks failing runs into smaller examples you can investigate.

A test is a composable LINQ query, called a **Checkr**. Its steps can generate
inputs, call your code, choose between operations, and check expectations.
The same building blocks work for a single function call or a workflow involving
several objects and changing state.

> QuickCheckr is all about state and side-effects, because most C# code is all about state and side-effects.

## How it works

* **Describe the behaviour.** Use QuickFuzzr to generate inputs, actions to call
  your code, and expectations to express its rules.
* **Explore variations.** Run the query with fresh inputs, or choose between
  operations to explore different paths. Stashed objects and tracked pools carry
  state between executions within a run.
* **Investigate a failure.** QuickCheckr replays the failing run and tries to
  simplify its sequence and inputs while preserving the failure. The report shows
  the remaining operations, relevant inputs, failed expectations, and a seed for
  reproduction.

You control the operations and expectations. This lets you check a result against
a reference implementation, verify a relationship between inputs and outputs, or
keep checking an invariant as a system changes. Async operations are supported
through execution, replay, and shrinking.

## Example: a stale shopping cart total

A shopping cart caches its total. Adding an item invalidates the cache, but
clearing the cart forgets to do so. A test that only adds items passes; reading the
total after adding and then clearing exposes the stale value.

The rule we want to check is simple: **the total must equal the sum of the current
items**, whatever operations came before it.

Here's the implementation with the cache invalidation bug:


```csharp
public class ShoppingCart
{
    private readonly List<int> items = [];
    private int? cachedTotal;

    public IReadOnlyList<int> Items => items;
    public int Total => cachedTotal ??= items.Sum();

    public void Add(int price)
    {
        items.Add(price);
        cachedTotal = null;
    }

    public void Clear()
    {
        items.Clear();
        // Bug: cachedTotal must also be invalidated.
    }
}
```
The Checkr chooses between adding an item and clearing the cart, then checks the total after every operation:
```csharp
from cart in Trackr.Stashed(() => new ShoppingCart())
from operation in Checkr.OneOf(
    from price in Checkr.Input("Price", Fuzzr.Int(1, 101))
    from add in Checkr.Act("Add item", () => cart.Add(price))
    select Case.Closed,
    Checkr.Act("Clear cart", () => cart.Clear()))
from total in Checkr.Expect("Total matches items",
    expect => expect.Equal(cart.Items.Sum(), cart.Total))
select Case.Closed;
```
**The report:**
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CreateReadMe.cs:23:1
 Original failing run:    3 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    42
 ------------------------------------------------------------
  Executed: Add item
   - Price = 13
 ------------------------------------------------------------
  Executed: Clear cart
 ==============================================
  !! Expectation Failed: Total matches items
       Expected: 0
       Actual: 13
 ==============================================
 Passed Expectations (during initial run)
 - Total matches items: 2x
 ------------------------------------------------------------
```
The expectation reads the total after adding an item, filling the cache. Clearing
the cart then leaves that cached value behind. The shrunk report isolates those
two operations and shows the mismatch.

Fixing this means invalidating the cached total in `Clear()`, just as `Add()`
already does. The test describes the rule; it doesn't prescribe the failing order
of operations.

## Running a Checkr

Install the package in your test project:

```bash
dotnet add package QuickCheckr
```

QuickCheckr targets .NET 8 and .NET 10. Add `using QuickCheckr;` and
`using QuickFuzzr;`, assign the query above to a variable named `checkr`, and run:

```csharp
checkr.Run(100.Runs(), 100.ExecutionsPerRun());
```

A **run** starts with fresh tracked state. An **execution** evaluates the query
once: in this example, one chosen operation and an expectation. The stashed cart
is shared across executions in a run, so QuickCheckr can explore how earlier
operations affect later ones.

A failing expectation or an unhandled action exception triggers shrinking and a
report. The runner throws so your test framework marks the test as failed.
To reproduce the report above, supply its seed:

```csharp
checkr.Run(42, 100.ExecutionsPerRun());
```

A bare `Run()` performs one run with one execution. Use
`await checkr.RunAsync(...)` for async workflows; overloads accept a cancellation
token.

## Building larger tests

Checkrs compose, so an operation can itself be a query with inputs, actions, and
expectations. Start with individual operations and combine them into a workflow.

* `Checkr.OneOfWhen` chooses among operations whose conditions hold.
* `Poolr` tracks collections of objects and lets operations select and update
  them, useful for relationships such as customers and orders.
* `Shrinkr.For<T>()` in `QuickCheckr.Shrinking` describes custom input
  classification, reduction, and traversal.
* `Trackr.Verify` checks accumulated state at the end of a run.
* `FileAs(...)` and the investigation APIs store failing seeds for later review.

For meaningful replay, create state through the Checkr workflow and keep
generated inputs and operations explicit. Each replay needs to reconstruct the
scenario from fresh state.

## Documentation

The [guide](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
builds up the workflow through executable examples backed by tests.

* [Start with inputs, actions, and expectations](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOutChapter/A_CheckringThingsOut.md).
* [Explore a domain invariant across coaches, courses, and changing skills](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md).
* [Browse the cookbook](https://github.com/kilfour/QuickCheckr/blob/main/CookBook/CookBook.md)
  for examples including parsing, scheduling, and accumulated-state checks.
* [Documentation index](https://github.com/kilfour/QuickCheckr/blob/main/Docs/ToC.md).
* [Changelog](https://github.com/kilfour/QuickCheckr/blob/main/change-log.md).

## Dependencies

* [QuickFuzzr](https://github.com/kilfour/QuickFuzzr): Random input generation.
* [QuickPulse.Show](https://github.com/kilfour/QuickPulse.Show): Object and value formatting.
* [QuickPulse](https://github.com/kilfour/QuickPulse): Diagnostics filtering and transformations; also used through QuickPulse.Show.

## Background

[Why QuickCheckr Exists](https://github.com/kilfour/QuickCheckr/blob/main/why-quickcheckr-exists.md)
tells the story behind the library and its roots in QuickCheck.

## License

[MIT](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE).
