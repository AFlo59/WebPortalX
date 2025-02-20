using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests;

public class UpdateProfileRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
} 