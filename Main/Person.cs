using Generators;

namespace Main;

[GenerateToString]
public partial class Person
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}
