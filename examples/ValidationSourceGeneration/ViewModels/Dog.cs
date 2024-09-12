using FluentValidation;

namespace ValidationSourceGeneration.ViewModels.FF;

public class Dog
{
    public string Name { get; set; }

    public double Long { get; set; }

    public double Height { get; set; }

    public double Weight { get; set; }
}

public class DogValidator : AbstractValidator<Dog>
{
    public DogValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Long).GreaterThan(0);
        RuleFor(x => x.Height).GreaterThan(0);
        RuleFor(x => x.Weight).GreaterThan(0);
    }
}
