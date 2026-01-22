using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class CreateTopicDto
{
    [Required(ErrorMessage = "標題為必填")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "描述為必填")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Significance { get; set; }
}
