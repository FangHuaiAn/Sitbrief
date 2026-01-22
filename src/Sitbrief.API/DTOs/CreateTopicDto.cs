using System.ComponentModel.DataAnnotations;

namespace Sitbrief.API.DTOs;

public class CreateTopicDto
{
    [Required]
    [MaxLength(300)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Description { get; set; }

    [MaxLength(1000)]
    public string? Significance { get; set; }
}
