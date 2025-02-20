using WebPortalX.Core.Models;
using WebPortalX.Core.Models.Requests;

namespace WebPortalX.Core.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<UserManager>> GetUserByIdAsync(long id);
        Task<ServiceResult<UserManager>> GetUserByEmailAsync(string email);
        Task<ServiceResult<UserManager>> RegisterUserAsync(UserManager user);
        Task<ServiceResult<UserManager>> AuthenticateAsync(string email, string password);
        Task<ServiceResult<UserManager>> UpdateUserAsync(long userId, UpdateUserRequest request);
        Task<ServiceResult> InitiatePasswordResetAsync(string email);
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ServiceResult> VerifyEmailAsync(string token);
    }
} 