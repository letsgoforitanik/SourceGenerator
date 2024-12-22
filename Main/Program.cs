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
            Address = "Bagmore, Kanchrapara",
            Department = "Electronics",
            Email = "letsgoforitanik@gmail.com",
            Salary = 60_000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            JoiningDate = DateTime.UtcNow
        };
        
        Console.WriteLine(employee);
    }
}