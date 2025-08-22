using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using MediatR;
using Users.Application.Contracts.Repositories;

namespace Users.Application.Users.Queries.GetUserDetails
{
    public record GetUserDetailsQuery(Guid Id) : IQuery<UserDetailsDto?>;

    public class GetUserDetailsQueryHandler : IQueryHandler<GetUserDetailsQuery, UserDetailsDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserDetailsQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserDetailsDto?> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (user == null)
                return null;

            return new UserDetailsDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.CreatedAt,
                user.LastModified,
                user.IsActive);
        }
    }
}

public record UserDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime? CreatedAt,
    DateTime? LastModifiedAt,
    bool IsActive); 