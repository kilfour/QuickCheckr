## Next version: 0.1.0 : Bringing It All Back Home
 
### Doing


### Todo
* !! Fix Deliberation and Vault Stuff
* Remove duplication in the runners.
* reorganize per feature
* mbt report
  ```text
  Falsified after 13 executions
  Minimal scenario: 4 executions
  Seed: 1636527909
  
  Scenario
  1. Add(64)
  2. Add(_)
  3. Add(_)
  4. Clear()
     !! ComputerSaysNo: ... tired now ...
  
  State at failure
  Model: { Result: 0 }
  SUT:   { Result: 64 }
  
  Observations
  - Result Matches: passed 12x

  Coverage
  - Add: executed 3x
  - Subtract: removed by shrinker
  - Clear: executed 1x
  ```
