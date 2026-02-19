# Gathering Evidence
[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  
  
This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.  
## Running Investigations
```csharp
checkr.GatherEvidence(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
    cfg => cfg with { FileAs = "GatheringEvidence" });
```
When you use the above method the following steps are taken:
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if used with a DeliberationPolicy.  
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
  
**Casefiles are considered *not similar* when:**  
- *Failure Description* is not equal.  
- *Execution Count* is not equal.  
- Any **Execution** differs, like so:  
  - Number of *Actions Performed* is not equal.  
  - Any *Action label* is not equal.  
  - Number of *Inputs* is not equal.  
  - Any *Input label* is not equal.  

Per default, the order of Executions does not matter for similarity.  
An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.  
## Show, Don't Tell
As stated in [the guide][Guide] we're going to use the [Bolting Horses][BoltingHorses] example,
to demonstrate how to find all the different ways the invariant can be violated.

So we're using the same model, and *almost* exactly the same Checkr.  
Changes:
- Added Shrink.None to the Name inputs. 
- Changed the `UpdateCoachSkills` a little to avoid the issue documented [here][J_TheCaseOfTheInvisibleCollection]:  
```csharp
public class GatheringEvidenceCheckr
{
    public static CheckrOf<Case> TheCheckr() =>
        from coaches in Trackr.PoolFor<Coach>()
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfWhen(
            Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
            Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
            Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
            Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
            Trackr.PoolWhen("(Coach, Course) to Assign",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;

    private static readonly CheckrOf<Case> CreateCoach =
        from name in Checkr.Input("Coach Name", NameFuzzr.For("Coach"), Try.None<string>())
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach)
        select Case.Closed;

    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills"
                , Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3)
                , () => [
                    Try.Default<IEnumerable<string>>(),
                    Try.Collection([[.. coach.Value.Skills]]) ]
                , Reduce.Collection([[], [.. coach.Value.Skills]]))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name", NameFuzzr.For("Course"), Try.None<string>())
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills",
                Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills))
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from assign in Checkr.Act("Assign Coach to Course",
                () => course.Value.AssignCoach(coach.Value))
            from isAssigned in Checkr.Expect("Coach is assigned",
                () => course.Value.AssignedCoach == coach.Value)
            from hasCourse in Checkr.Expect("Coach has Course",
                () => coach.Value.AssignedCourses.Contains(course.Value))
            select Case.Closed;

    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(),
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;
}
```
For the code on display here, running this checkr using the code snippet at the top of this doc, 
consistently results in two different reports.  
```text
------------------------------------------------------------
 Original failing run:    10 executions
 Minimal failing case:    4 executions (after 11 shrinks)
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-3
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    12 executions
 Minimal failing case:    6 executions (after 11 shrinks)
 Seed:                    414682113
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-2
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-2, Course-1 )
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "TDD" ]
     Redux                    [ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Course Name Set: 2x
 - Coach Name is set: 2x
 - Course Required Skills Set: 2x
 - Coach is assigned: 4x
 - Coach has Course: 4x
 - Assigned Coach Must Be Suitable: 6x
 - Coach Skills Set: 2x
 ------------------------------------------------------------
```
## An Observation
When I first implemented this feature, I was quite puzzled.
Surely there are more ways this Checkr can fail, I thought.  

It took a bit of investigating as to why this is not the case. Let me give you an example of what is going on.

If you look at the second report, the one with 6 executions, surely there is a similar case,
but instead of updating the Coach skills, we update the Course skills and that would also fail.
Now that's a new way of failing the Checkr, right?

Wrong!  

Below is the report of the other failure (the one with 4 executions) with shrinking disabled,
but I'll save you the *bit of investigating* and give you a recap:
- Execution 03: Create Course-1
- Execution 06: Create Coach-3
- Execution 07: Update Coach Skills to [ "DomainDrivenDesign", "JavaScript" ]
- Execution 08: Update Course Skills to [ "DomainDrivenDesign" ]
- Execution 09: Assign Coach-3 to Course-1
- Execution 10: Update Course Skills to [ "JavaScript", "TDD", "CI/CD" ]

As you can see, this is exactly the case described above.  
The Shrinker however managed to collapse that one down to only four executions, so it is in fact covered by the first report.
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\B_PuttingACaseOnTheBoard\E_GatheringEvidence\GatheringEvidenceObservation.cs:37:1
 Original failing run:    10 executions
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-1
   - Input: Coach Name  = "Coach-1"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-1
   - Input: Coach Skills           = [ "ASP.NET" ]
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = Course-1
   - Input: Course Name  = "Course-1"
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-2
   - Input: Coach Name  = "Coach-2"
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "UnitTesting" ]
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-3
   - Input: Coach Name  = "Coach-3"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-3
   - Input: Coach Skills           = [ "DomainDrivenDesign", "JavaScript" ]
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "DomainDrivenDesign" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   -  Pool: (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "JavaScript", "TDD", "CI/CD" ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```

[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[TheEvidenceVault]: ../D_TheEvidenceVault/TheEvidenceVault.md

[Guide]: ../../A_Guide/Guide.md

[BoltingHorses]: ../../A_Guide/E_HorsesAgain/BoltingHorses.md

[J_TheCaseOfTheInvisibleCollection]: ../../M_HoneyIShrunkTheInputs/B_Collections/TheExamples/J_TheCaseOfTheInvisibleCollection.md
