
// Data Structures
public class Person
{
    private static int lastId = 0;
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Person(string name)
    {
        Id = ++lastId;
        Name = name;
    }
}
