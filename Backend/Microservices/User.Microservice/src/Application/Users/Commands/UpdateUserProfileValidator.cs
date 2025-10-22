using System;
using FluentValidation;

namespace Application.Users.Commands
{
    public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(100)
                .EmailAddress();

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(255)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("AvatarUrl must be a valid absolute URL.");
        }
    }
}
