using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.C_ModelBasedTesting.Sub;


[DocFile]
[DocModelHeader]
[DocExample(typeof(RolodexOracle))]
[DocBoldHeader("SUT")]
[DocExample(typeof(Rolodex))]
[DocExample(typeof(Person))]
[DocTestrHeader]
[DocTestr]
[DocReportHeader]
[DocReport]
public class B_Rolodex : QuickTestrModelRunTest<B_Rolodex>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("322254521, 100.ExecutionsPerRun()")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Rolodex")
            .Model(() => new RolodexOracle())
            .Sut(() => new Rolodex())
            .Operation("Add", NameFuzzr,
                (model, a) => model.Add(a.First, a.Last),
                (sut, a) => sut.Add(a.First, a.Last))
            .Operation("Delete All By Name",
                model => model.People.Any(), // precondition for this operation
                model =>
                    from names in Fuzzr.FromEach(model.People, a => Fuzzr.OneOf(a.FirstName, a.LastName))
                    from name in Fuzzr.OneOf(names)
                    select name,
                (model, a) => model.DeleteAllByName(a),
                (sut, a) => sut.DeleteAllByName(a))
            .Observe("People Match", PeopleMatch, a => a.Trace())
            .StoreCaseFiles(journalist)
            .Run(322254521, 100.ExecutionsPerRun());

    protected override void Verify(Article article)
    {
        Assert.Equal("People Match", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(322254521, article.Seed());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(6, article.Total().Actions());
        Assert.Equal(3, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal("-QTM-Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("-QTS-Add", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("-QTM-Delete All By Name", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("-QTS-Delete All By Name", article.Execution(3).Action(2).Read().Label);
        Assert.Equal("\"w\"", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Model:", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("Sut:  ", article.Execution(3).Trace(2).Read().Label);
    }

    private static FuzzrOf<(string First, string Last)> NameFuzzr =>
        from first in Fuzzr.String()
        from last in Fuzzr.String()
        select (First: first, Last: last);


    private static bool PeopleMatch(RolodexOracle model, Rolodex sut)
    {
        if (model.People.Count != sut.People.Count)
            return false;
        for (int i = 0; i < model.People.Count; i++)
        {
            var modelPerson = model.People[i];
            var sutPerson = sut.People[i];
            if (modelPerson.FirstName != sutPerson.FirstName)
                return false;
            if (modelPerson.LastName != sutPerson.LastName)
                return false;
        }
        return true;
    }

    [CodeExample]
    public class RolodexOracle
    {
        private readonly List<Person> people = [];

        public IReadOnlyList<Person> People => people;

        public void Add(string firstName, string lastName)
        {
            people.Add(new Person { FirstName = firstName, LastName = lastName });
        }

        public void DeleteAllByName(string name)
        {
            people.RemoveAll(person => person.FirstName == name || person.LastName == name);
        }
    }
}

[CodeExample]
public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

[CodeExample]
public class Rolodex
{
    private readonly List<Person> people = [];

    public IReadOnlyList<Person> People => people;

    public void Add(string firstName, string lastName)
    {
        people.Add(new Person { FirstName = firstName, LastName = lastName });
    }

    public void DeleteAllByName(string name)
    {
        for (int i = 0; i < people.Count(); i++)
        {
            if (people[i].FirstName == name || people[i].LastName == name)
            {
                people.RemoveAt(i);
            }
        }
    }
}
