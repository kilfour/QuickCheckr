# DoctorsOrders

A small Northwind-style Customers/Orders domain backed by a private in-memory SQLite database per repository. No service or external database is required. This project currently contains the implementation and example unit tests; the Checkr will be added separately.

`OrderRepository` enables all four intentional cache defects by default. Pass an individual `CacheBugs` flag to isolate one, or `CacheBugs.None` for correct behavior. The defects omit country from cache keys only when customer and both date bounds are supplied, retain empty query results after inserts, and fail to invalidate the previous customer after moving an order. The fourth defect resets refresh tracking after a SQLite query error while retaining cached snapshots. Warm a query, insert a matching order, issue a name-pattern query with `LikeEscape: "!!"`, then repeat the original query: it returns stale data. SQLite itself rejects the two-character LIKE escape. No failure callbacks, extra connections, or locks are needed. Tests named `Bug_*` characterize these defects rather than asserting the intended contract.

Customer name patterns use SQLite LIKE with a configurable single-character escape (backslash by default). Date bounds are inclusive. Queries use exact customer/country matching and order by date then ID before paging. Keep a repository alive across operations to observe the cache histories; dispose it between runs. The repository is intended for sequential use.

Tests are skipped unless `QUICKCHECKR_DEMO_TESTS` is `1` or `true`. In PowerShell:

```powershell
$env:QUICKCHECKR_DEMO_TESTS = '1'
dotnet test QuickCheckr.Demo/QuickCheckr.Demo.csproj
Remove-Item Env:QUICKCHECKR_DEMO_TESTS
```




