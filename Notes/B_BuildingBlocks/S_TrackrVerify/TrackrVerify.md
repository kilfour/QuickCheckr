# Trackr Verify
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
    // using quickfuzzr below, but really it's a SUT
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from verify in Trackr.Verify("Contains 3", () => observer.Contains(3)) // <= gen = [1, 3) : fails
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\B_BuildingBlocks\S_TrackrVerify\TrackrVerify.cs:19:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 ==========================================
  !!  The Verifier Disagrees: Contains 3
 ==========================================
```
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Multiple
 Location:                Notes\B_BuildingBlocks\S_TrackrVerify\TrackrVerify.cs:53:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 =============================================================
  !!  The Verifier Disagrees: Rolls: Contains 3, Contains 4
 =============================================================
```
