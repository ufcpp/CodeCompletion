namespace TrialWpfApp.Models;

public class A
{
    public int Id { get; set; }
    public string Name { get; set; }
    public B Item { get; set; }
}

public class B
{
    public int Number { get; set; }
    public C[] Points { get; set; }

    public int[] Values { get; set; }
    public string[] Attributes { get; set; }
}

public class C
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Description { get; set; }
}
