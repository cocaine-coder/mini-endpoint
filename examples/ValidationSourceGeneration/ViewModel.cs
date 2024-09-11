using FluentValidation;

namespace ValidationSourceGeneration;

public record Person(string Name, int Age);

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0);
    }
}
