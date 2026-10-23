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
