using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    public string Password { get; set; } = string.Empty;
}
