using DartsPlayer.Match.Requests;
using FluentValidation;

namespace DartsPlayer.Match.Validators;

public class UpdateScoreValidator : AbstractValidator<UpdateScoreRequest>
{
    public UpdateScoreValidator()
    {
        RuleFor(x => x.Throws)
            .Must(throws => throws.Count == 3)
            .WithMessage("Exactly three throws must be provided.");

        RuleForEach(x => x.Throws)
            .ChildRules(throwItem =>
            {
                throwItem.RuleFor(x => x.Score)
                    .InclusiveBetween(0, 20)
                    .When(x => x.Score != 25)
                    .WithMessage("Score must be between 0 and 20, or 25.");

                throwItem.RuleFor(x => x.Multiplier)
                    .InclusiveBetween(1, 3)
                    .WithMessage("Multiplier must be 1, 2, or 3.")
                    .When(x => x.Score != 25);

                throwItem.RuleFor(x => x.Multiplier)
                    .InclusiveBetween(1, 2)
                    .WithMessage("When score is 25, multiplier must be 1 or 2.")
                    .When(x => x.Score == 25);
            });
    }
}