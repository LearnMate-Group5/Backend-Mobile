using System;
using FluentValidation;

namespace Application.Users.Commands
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(70)
                .EmailAddress();

            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}
