namespace Main;

partial class Program
{
    public static void Main()
    {
        var person = new Person
        {
            FirstName = "Anik",
            LastName = "Banerjee"
        };
        
        Console.WriteLine(person);
    }
}