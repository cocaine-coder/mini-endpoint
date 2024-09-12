using FluentValidation;

namespace ValidationSourceGeneration.ViewModels
{
    public class CatValidator : AbstractValidator<Models.Cat>
    {
        public CatValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Long).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.Weight).GreaterThan(0);
        }
    }
}

namespace ValidationSourceGeneration.ViewModels.BB
{
    public class Cat
    {
        public string Name { get; set; }

        public double Long { get; set; }

        public double Height { get; set; }

        public double Weight { get; set; }
    }

    public class CatValidator : AbstractValidator<Cat>
    {
        public CatValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Long).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.Weight).GreaterThan(0);
        }
    }
}
