using FluentValidation;
using events.Models;

public class EventRegistrationDTOValidator : AbstractValidator<EventRegistrationDTO>
{
    public EventRegistrationDTOValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.EventName)
            .NotEmpty().WithMessage("Event name is required.");

        RuleFor(x => x.EventDate)
            .NotEmpty().WithMessage("Event date is required.");

        RuleFor(x => x.ConfirmEmail)
            .NotEmpty().WithMessage("Confirm email is required.");

        RuleFor(x => x.DaysAttending)
            .NotEmpty().WithMessage("Days attending is required.");

        RuleFor(x => x.FullName)
            .Must(name => !name.Contains("Garry") && !name.Contains("Luke"))
            .WithMessage("{PropertyValue} is not allowed in the full name.");

        RuleFor(x => x.EventName)
            .Must(value => 
                new[] { "C# Conference", "WebAPI Workshop", ".NET Hangout" }.Contains(value))
            .WithMessage("Event name must be one of the specified values.");

        RuleFor(x => x.EventDate)
            .GreaterThan(DateTime.Now).WithMessage("Event date must be in the future.");

        RuleFor(x => x.ConfirmEmail)
            .Equal(x => x.Email).WithMessage("Email addresses do not match.");

        RuleFor(x => x.DaysAttending)
            .InclusiveBetween(1, 7).WithMessage("Number of days attending must be between 1 and 7.");
    }
}
