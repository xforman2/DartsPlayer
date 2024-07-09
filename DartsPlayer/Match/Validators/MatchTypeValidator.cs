using DartsPlayer.Match.Requests;
using FluentValidation;

namespace DartsPlayer.Match.Validators;

public class MatchTypeValidator: AbstractValidator<MatchTypeRequest>
{
    public MatchTypeValidator()
    {
        RuleFor(x => x.MatchType).IsInEnum()
            .WithMessage("Invalid match type.");
    }
}