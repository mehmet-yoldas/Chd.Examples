using Chd.AutoUI.Extensions;
using Chd.Security.DTOs;
using Chd.Security.Models;

// Demo implementation of IUserTokenProvider.
// In a real application, inject your DbContext or user service and verify credentials securely.
class UserRepoesitory : IUserTokenProvider
{
    public Task<UserDTO?> GetUserTokenInfoAsync(UserModel userDTO)
    {
        var userResultDto = new UserDTO { Roles = new(), UserName = userDTO.UserName, ExpirationSecond = 50000000 };
        if (userDTO.Password != "test")
            return Task.FromResult<UserDTO?>(null);

        switch (userDTO.UserName) // In a real app, validate credentials and fetch roles from your user store
        {
            case "Admin":
                userResultDto.Roles.AddRange(new List<string> { "User", "Admin" });
                break;
            case "Manager":
                userResultDto.Roles.AddRange(new List<string> { "User", "Manager" });
                break;
            case "User":
                userResultDto.Roles.AddRange(new List<string> { "User" });
                break;
        }

        return Task.FromResult<UserDTO?>(userResultDto);
    }
}