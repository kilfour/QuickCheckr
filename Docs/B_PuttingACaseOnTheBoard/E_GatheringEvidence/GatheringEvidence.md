# Gathering Evidence
[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  
  
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
  
## Gathering Evidence Case File Compare
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
- Changed the `UpdateCoachSkills` a little to avoid the issue documented [here][J_TheCaseOfTheInvisibleList]:  
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
        from name in Checkr.Input("Coach Name", NameFuzzr.For("Coach"), Shrink.None<string>)
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach)
        select Case.Closed;

    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills", SkillFuzzr.Many
                , () => [
                    Shrink.Default<IEnumerable<string>>(),
                    Shrink.Collection([[.. coach.Value.Skills]]) ]
                , Reduce.Values<IEnumerable<string>>([[], [.. coach.Value.Skills]]))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name", NameFuzzr.For("Course"), Shrink.None<string>)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills", SkillFuzzr.Many)
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
Running the Checkr using the code snippet at the top of this file, consistently results in five different reports.  
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

[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[TheEvidenceVault]: ../D_TheEvidenceVault/TheEvidenceVault.md

[Guide]: ../../A_Guide/Guide.md

[BoltingHorses]: ../../A_Guide/E_HorsesAgain/BoltingHorses.md

[J_TheCaseOfTheInvisibleList]: ../../M_HoneyIShrunkTheInputs/B_Collections/A_Lists/TheExamples/J_TheCaseOfTheInvisibleList.md
