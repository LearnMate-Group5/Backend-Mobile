using FluentValidation;

namespace Application.Users.Commands
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().MaximumLength(70).EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(70);
        }
    }
}