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
                .WithMessage("'Name' must not be empty when provided.")
                .MaximumLength(100)
                .When(x => x.Name is not null);

            RuleFor(x => x.Email)
                .MaximumLength(100)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.DateOfBirth)
                .Must(date => !date.HasValue || date.Value <= DateTime.Today)
                .WithMessage("Date of birth cannot be in the future.");

            RuleFor(x => x.Gender)
                .MaximumLength(20)
                .When(x => x.Gender is not null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.AvatarBase64)
                .Must(BeValidBase64)
                .WithMessage("Avatar must be a valid base64 string.")
                .When(x => !string.IsNullOrWhiteSpace(x.AvatarBase64));

            RuleFor(x => x)
                .Must(HasAtLeastOneChange)
                .WithMessage("At least one field must be provided.");
        }

        private static bool BeValidBase64(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            Span<byte> buffer = new Span<byte>(new byte[value.Length]);
            return Convert.TryFromBase64String(value, buffer, out _);
        }

        private static bool HasAtLeastOneChange(UpdateUserProfileCommand command)
        {
            return command.Name is not null
                   || command.Email is not null
                   || command.DateOfBirth.HasValue
                   || command.Gender is not null
                   || command.PhoneNumber is not null
                   || command.AvatarBase64 is not null;
        }
    }
}
