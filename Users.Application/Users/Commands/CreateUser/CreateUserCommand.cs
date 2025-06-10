using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using MediatR;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PhoneNumber) : ICommand<Guid>;

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20);
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists.");
            }

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = User.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.PhoneNumber);

            await _userRepository.AddAsync(user, cancellationToken);

            return user.Id;
        }
    }
} 