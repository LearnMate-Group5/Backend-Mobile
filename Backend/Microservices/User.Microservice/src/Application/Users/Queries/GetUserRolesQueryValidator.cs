using FluentValidation;

namespace Application.Users.Queries
{
    public class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
    {
        public GetUserRolesQueryValidator()
        {
            RuleFor(query => query.UserId)
                .NotEmpty()
                .WithMessage("User id is required.");
        }
    }
}
