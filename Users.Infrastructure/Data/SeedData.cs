using Users.Domain.Models;

namespace Users.Infrastructure.Data;

public static class SeedData
{
    public static IEnumerable<User> GetUsers()
    {
        User user1 = User.Create("Nguyen Duy ", "Hieu", "nguyenduyhieu202@gmail.com", "$2a$12$vve2hraeuDk00wsyTujCfO9RIQQiKpmSACRLbImNBD6ppu.mKx5gK", "0913552733");
        User user2 = User.Create("Nguyen Van ", "Anh", "nguyenduyanh202@gmail.com", "$2a$12$OTPZe3SDTVjFWNGmSLEMwuse84A/H3uNLfSge92Q3JdAZfm2FhNjO", "0913553144");

        return new List<User>
        {
           user1, user2
        };
    }
} 