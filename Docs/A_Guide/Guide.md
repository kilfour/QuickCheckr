# Guide
**Welcome to the QuickCheckr Guide.**  

This document walks through the core ideas of QuickCheckr,
from the basics of building a Checkr, to stateful testing, branching behaviour, pools, shrinking,
leading up to a real-world scenario.  
Each chapter builds on the previous one, introducing new concepts through executable examples.

QuickCheckr is mainly designed to test behaviour that unfolds over time.
It can check individual function calls,
but it also explores sequences of actions and reduces failures to the smallest scenario that still breaks.

  
## Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and Executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [Choosing the Next Move][ChoosingTheNextMove]
  -  A brief introduction to behavioural branching.
- [In Pools We Trust][InPoolsWeTrust]
  - An overview of pools and their usage.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  

[CheckringThingsOut]: A_CheckringThingsOutChapter/A_CheckringThingsOut.md

[TheStashRunsAndExecutions]: B_TheStashRunsAndExecutionsChapter/A_TheStashRunsAndExecutions.md

[ChoosingTheNextMove]: C_ChoosingTheNextMove/ChoosingTheNextMove.md

[InPoolsWeTrust]: D_InPoolsWeTrust/InPoolsWeTrust.md

[BoltingHorses]: E_HorsesAgain/BoltingHorses.md
