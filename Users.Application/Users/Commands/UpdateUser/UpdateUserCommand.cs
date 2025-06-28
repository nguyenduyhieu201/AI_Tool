using BuildingBlock.CQRS;
using System;
using System.ComponentModel.DataAnnotations;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.UpdateUser
{
    /// <summary>
    /// Command để cập nhật thông tin user
    /// </summary>
    public record UpdateUserCommand
    (
        Guid Id, 
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber
    ) : ICommand<Guid>;
}