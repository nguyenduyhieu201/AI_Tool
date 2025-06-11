using BuildingBlock.CQRS;
using FluentValidation;

namespace Users.Application.Users.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password) : ICommand<LoginResponseDto>;

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100);
        }
    }

    public record LoginResponseDto(
        Guid UserId,
        string FirstName,
        string LastName,
        string Email,
        string Token);
} 