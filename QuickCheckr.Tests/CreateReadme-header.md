> **CSI: .NET**

Property-based testing for the pragmatic.

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)

**QuickCheckr** is a C# library for exploring how your code behaves with generated
inputs and sequences of operations. You describe what the system can do and what
should remain true. QuickCheckr runs those descriptions, looks for failures, and
shrinks failing runs into smaller examples you can investigate.

> QuickCheckr is all about state and side-effects, because most C# code is all about state and side-effects.

A test is a composable LINQ query, called a **Checkr**. Its steps can generate
inputs, call your code, choose between operations, and check expectations.
The same building blocks work for a single function call or a workflow involving
several objects and changing state.

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
