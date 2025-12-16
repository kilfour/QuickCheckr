namespace QuickCheckr.Tests.Experimental.P_ThePoolInvestigation.Model;

public class Storage
{
    private int nextId = 0;
    public List<Person> People { get; set; } = [];

    public int AddPerson(string name)
    {
        nextId++;
        People.Add(new Person { Id = nextId, Name = name });
        return nextId;
    }

    public void UpdateName(int id, string name)
        => People.Single(a => a.Id == id).Name = name;

    public Person GetById(int id)
        => People.Single(a => a.Id == id);
}