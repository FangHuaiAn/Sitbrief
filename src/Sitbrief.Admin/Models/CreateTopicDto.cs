using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class CreateTopicDto
{
    [Required(ErrorMessage = "標題為必填")]
    [MaxLength(300, ErrorMessage = "標題長度不能超過 300 字元")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "描述為必填")]
    [MaxLength(1000, ErrorMessage = "描述長度不能超過 1000 字元")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "意義/影響長度不能超過 1000 字元")]
    public string? Significance { get; set; }
}
