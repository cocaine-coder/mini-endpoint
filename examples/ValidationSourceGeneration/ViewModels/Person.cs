using FluentValidation;

namespace ValidationSourceGeneration.ViewModels;

public record Person(string Name, int Age);

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0);
    }
}
