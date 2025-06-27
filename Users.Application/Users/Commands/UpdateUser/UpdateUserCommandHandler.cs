using BuildingBlock.CQRS;
using System;
using System.Threading;
using System.Threading.Tasks;
using Users.Application.Contracts.Repositories;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.UpdateUser
{
    /// <summary>
    /// Handler xử lý cập nhật thông tin user
    /// </summary>
    public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        public UpdateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Xử lý command cập nhật user
        /// </summary>
        public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Tìm user theo ID
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new Exception($"User with ID {request.Id} not found");
            }

            // Cập nhật thông tin user
            user.Update(
                request.FirstName,
                request.LastName,
                request.PhoneNumber
            );

            // Lưu thay đổi
            await _userRepository.UpdateAsync(user);

            return user.Id;
        }
    }
} 