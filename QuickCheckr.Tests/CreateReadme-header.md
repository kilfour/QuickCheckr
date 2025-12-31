> **CSI: .NET**

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)

**QuickCheckr** is inspired by Haskell's QuickCheck, and as such, it is a Property Based Testing framework.  
But it is not a straight-up clone. I could not write one of those anyway. I can't read the maths.  
Instead it took the ideas of QuickCheck, not the algorithms, and was built from the ground up,
iteratively, over a long period of time.

So if you are looking for a mathematically correct, powerfull shrinking algorithm, and are dealing with pure functional code,
there are better solutions out there. FsCheck comes to mind, even if it is slightly unwieldly in C#.  
But if you're dealing with the impure, messy world of day to day .NET code ...  

> QuickCheckr is all about state and side-effects, because most C# code is all about state and side-effects.

Now there *is* a non-trivial *learning-curve*. 
Because in order to get the shrinker to work, we have to encapsulate all state under test. QuickCheckr uses `LINQ` composition to achieve that, which is atleast familiar to most C# devs.  
But it does come with it's own dialect and the idea of *stateful LINQ* might feel strange at first sight.

All that said, below is a first, granted, contrived, example. A more [real world one](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md) can be found at the end of the guide. But before you get to that it is advised to start at the [beginning](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOut/CheckringThingsOut.md) of said guide.

