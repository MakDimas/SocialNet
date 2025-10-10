using SocialNet.Core.Dtos.UserDtos;
using SocialNet.Domain.Identity;

namespace SocialNet.Core.Profiles;

public static class UserManualProfile
{
    public static UserResponseDto FromUserToUserResponseDto(this User user) =>
        new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Description = user.Description,
            Age = user.Age
        };

    public static void UpdateUserInfo(this User user, UpdateUserDto updateDto)
    {
        if (!string.IsNullOrEmpty(updateDto.FirstName) && updateDto.FirstName != user.FirstName
            && !string.IsNullOrEmpty(updateDto.LastName) && updateDto.LastName != user.LastName)
        {
            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.UserName = $"{user.FirstName.Trim()}.{user.LastName.Trim()}";

            UpdateUserPostsInfo(user);
        }
        else if (!string.IsNullOrEmpty(updateDto.FirstName) && updateDto.FirstName != user.FirstName)
        {
            user.FirstName = updateDto.FirstName;
            user.UserName = $"{user.FirstName.Trim()}.{user.LastName.Trim()}";

            UpdateUserPostsInfo(user);
        }
        else if (!string.IsNullOrEmpty(updateDto.LastName) && updateDto.LastName != user.LastName)
        {
            user.LastName = updateDto.LastName;
            user.UserName = $"{user.FirstName.Trim()}.{user.LastName.Trim()}";

            UpdateUserPostsInfo(user);
        }

        if (!string.IsNullOrEmpty(user.Email) && updateDto.Email != user.Email)
        {
            user.Email = updateDto.Email;
            user.NormalizedEmail = user.Email.ToUpper();

            foreach (var post in user.Posts)
            {
                post.UserEmail = user.Email!;
            }
        }

        user.PhoneNumber = updateDto.PhoneNumber;
        user.Description = updateDto.Description;
        user.Age = updateDto.Age;
    }

    private static void UpdateUserPostsInfo(User user)
    {
        foreach (var post in user.Posts)
        {
            post.UserName = $"{user.FirstName} {user.LastName}";
            post.HomePageUrl = $"http://socialnetclient-cxggdqdrfwg6h0bd.northeurope-01.azurewebsites.net/user/{user.UserName!.ToLower()}/{user.Id}";
        }
    }
}
