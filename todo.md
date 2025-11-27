### Main
* QOL (major): throw exception when multiple writes occur to the same memorybank with the same key on the same execution
  * check OneOf, more than one in same checkr => same key => problem
  * guard against reuse keys all over
* port
  * the remaining collection shrinking tests
  * object shrinking
* async : Act, add overloads ...If and Action variants
* test ActCarefully more
* Assay
* Analyze
* TestifyProvenWhen

### Minor
!! fix InputShrinking shrinkCount

InputShrinker.Run never increments shrinkCount (Runners/InputShrinking/InputShrinker.cs (lines 8-34)), so the shrink statistics always report zero even when clues are found. If that metric matters later (docket, case file), it needs to be wired up.

* Nested InputShrinkingClue ?
* Verdict nullabillity => meh


## Community
* Discord Server: Checkrs Court
  * Start Here
    #welcome-to-court
    #rules-and-ethics
    #announcements
  * QuickCheckr Support
    #ask-a-checkr
    #minimal-failing-cases
    #shrinking-talk
    #api-design-and-dx
    #examples-and-scripts
  * Learning
    #guide-discussion
    #how-to-write-a-checkr
    #domain-testing
    #shrinking-theory
  * The Courtroom 
    #the-witness-stand (people share their bug stories)
    #the-evidence-room (paste reports, stack traces)
    #the-library-of-cases (share reusable Checkrs)
  * Development
    #issues-and-roadmap
    #ideas-and-proposals
    #internal-design
    #release-prep

Role Names 
* Clerk – mods
* Investigators – contributors
* Barristers – people helping others
* Witnesses – new members
* Defendants – people who bring buggy code
* Justices – core maintainers (you!)

Welcome Message

Welcome to Checkrs Court!

A place where cases are heard, minimal failing inputs are uncovered,
and invariants finally get the justice they deserve.

* Ask your first question in #ask-a-checkr
* Share your shrink reports in #the-evidence-room
* Browse examples in #the-library-of-cases

The court is now in session !

1. Publish the first “Wow” Example as a Standalone Post
The Bolting Horses invariant drift example is your version of the “10-line bug FsCheck found.”
People love that content.
Post it on:
/r/csharp
/r/dotnet
Hacker News (works shockingly well for invariant-drift stories)
dev.to
LinkedIn (your network is your early adopters)

Structure of the post:
Short intro:
“I’ve been building a new property-based testing tool for C#, QuickCheckr. It found a real-world invariant drift bug with almost no effort.”
The buggy code (minimal)
Keep it readable.
The failing QuickCheckr script
As short as possible.
The shrinking report
This is the magic moment.
A single sentence:
“If you want to try it out, here’s the repo.”
This one post can easily pull 50–200 people in a day if framed correctly.

2. Build a “Weird Bugs” Series (your trump card)

You have a HUGE advantage:
You’re slowly sitting on a treasure chest of shrinked failing cases.

Make a recurring format:
“QuickCheckr finds another bizarre bug #1: Bolting Horses”
“QuickCheckr finds another bizarre bug #2: BugHouse – Delayed Detonation”
“QuickCheckr finds another bizarre bug #3: Multi-method stateful trap”

These work because:
They’re short
They’re entertaining
They don’t require people to understand the entire tool
They show value better than 1000 words of documentation
This is exactly how Hypothesis (Python), FsCheck, and jqwik grew attention.

4. A Cookbook Repo: QuickCheckr.Cases

People LOVE copying examples.

Create a repo:
QuickCheckr.Cases
with folders:

Stateful
Async
DomainBugs
WebApi
Collections
Invariants
Oracles
Minimal Repros
“Found in the wild”

Every example is:
1 file
1 Checkr
1 report

This will grow by itself once you have contributors.

5. Do What You Already Do: Teaching
But externalize it.
Make a tiny free PDF:
“Property-Based Testing For Mortals – With C# and QuickCheckr”
10–12 pages.
Use content from your Guide.md and Notes.md.
It doesn’t need to be polished.
Just enough to be shareable.
People eat this up because good PBT resources in C# are basically nonexistent.

6. Small Screenshots of Reports
Shrinking reports are inherently social-media-friendly.
If you post this:
------------------------------------------------------------
 Executed (15): Update Course Skills
   - Input: Course Skills = [ _ ]
 !! Spec Failed: Assigned Coaches Must Be Suitable
------------------------------------------------------------
…people WILL click.
You don't need images. The ASCII art is the visual.

7. Start With a Micro Roadmap
People don’t follow tools.
They follow momentum.

Post a GitHub project board with:
0.1.0 – “First public pre-release”
0.2.0 – “Async support”
0.3.0 – “Differential testing package”
0.4.0 – “WebApi boundary testing”
0.5.0 – “Visual shrinking explorer” (even if far away)

Momentum = trust.

8. Don’t build “a community”—build 5 superfans
Communities start with:
2 contributors
1 evangelist
1 bug hunter
1 person giving feedback weekly

Where do you find them?
By DMing the commenters who post:
“Interesting—does this work for X?”
…those are your first superfans.
Invite them to Discord.
Ask what breaks for them.
Ask what they wish existed.
People love being early.

9. Record ONE 10-minute demo video
Screen recording.
No fancy editing.
Just you, QuickCheckr, a failing example, and shrinking.
This is all you need to get 100 new users:

Title:
“QuickCheckr: C# property-based testing with real shrinking. Here’s a failing domain invariant in 10 lines.”
Upload to YouTube and link from:
Reddit posts
GitHub README
Discord
Documentation
This works extremely well.

10. The QuickCheckr Narrative Matter
You’re not building just a tool.
You’re telling a story developers love:
Shrinking done right
Transparent diagnostics
Not FsCheck
Not Hypothesis
Not QuickCheck clone
Designed for real-world domains
Modern C# LINQ query syntax
Built by someone who teaches and cares about clarity
Make that the project identity.
