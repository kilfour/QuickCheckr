# Guide
**Welcome to the QuickCheckr Guide.**  
This document walks through the core ideas of QuickCheckr,
from the basics of building a Checkr, to stateful testing, branching behaviour, pools, shrinking,
leading up to a real-world scenario.  
Each chapter builds on the previous one, introducing new concepts through executable examples.  
## Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [ChoosingTheNextMove]
  -  A brief introduction to behavioural branching.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  

[CheckringThingsOut]: A_CheckringThingsOut/CheckringThingsOut.md

[TheStashRunsAndExecutions]: B_TheStashRunsAndExecutions/TheStashRunsAndExecutions.md

[ChoosingTheNextMove]: C_ChoosingTheNextMove/ChoosingTheNextMove.md

[BoltingHorses]: E_HorsesAgain/BoltingHorses.md
