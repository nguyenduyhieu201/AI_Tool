using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

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

    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await ValidateAsync(request.Email, request.Password, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponseDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                token);
        }

        private async Task<User?> ValidateAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            if (!_passwordHasher.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }
    }
} 