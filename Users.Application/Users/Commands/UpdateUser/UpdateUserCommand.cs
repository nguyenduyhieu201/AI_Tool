using BuildingBlock.CQRS;
using System;
using System.ComponentModel.DataAnnotations;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.UpdateUser
{
    /// <summary>
    /// Command để cập nhật thông tin user
    /// </summary>
    public class UpdateUserCommand : ICommand<Guid>
    {
        /// <summary>
        /// ID của user cần cập nhật
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Tên của user
        /// </summary>
        [Required(ErrorMessage = "FirstName is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "FirstName must be between 2 and 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// Họ của user
        /// </summary>
        [Required(ErrorMessage = "LastName is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "LastName must be between 2 and 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// Email của user
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Số điện thoại của user
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }

    }
}