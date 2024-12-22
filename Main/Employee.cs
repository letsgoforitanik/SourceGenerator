using Generators;

namespace Main;


[GenerateToString]
public partial class Employee
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required int Age { get; init; }
}


public partial class Employee
{
    public required string Email { get; init; }
    public required string Address { get; init; }
}


public partial class Employee
{
    public required double Salary { get; init; }
}

