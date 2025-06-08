using BuildingBlock.CQRS;
using Users.API.DTOs;

namespace Users.API.Users.UserLogin
{
    public record UserLoginCommand(UserLoginDTO userLoginDto);

    public class UserLoginHandler
    {
    }
}
