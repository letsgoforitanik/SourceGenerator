using Generators;

namespace Main;


[GenerateFilter(Include = "FirstName")]
public partial class Employee
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required int Age { get; init; }
    public required double Score { get; init; }
}


