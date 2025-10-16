using System;
using FluentValidation;

namespace Application.Users.Commands
{
    public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleCommand>
    {
        public UpdateUserRoleValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.RoleName)
                .NotEmpty()
                .MaximumLength(50)
                .Must(role => UserRoleCatalog.TryGetCanonicalName(role, out _))
                .WithMessage(_ => $"Supported roles: {string.Join(", ", UserRoleCatalog.SupportedRoles)}");
        }
    }
}
