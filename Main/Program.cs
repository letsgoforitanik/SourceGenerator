namespace Main;

class Program
{
    public static void Main()
    {
        var employee = new Employee
        {
            FirstName = "Aniruddha",
            MiddleName = "Prasad",
            LastName = "Banerjee",
            Age = 33,
            Score = 0
        };
        
        Console.WriteLine(employee);
    }
}