using System.ComponentModel.DataAnnotations;

namespace Sitbrief.API.DTOs;

public class LinkTopicsDto
{
    [Required]
    public List<int> TopicIds { get; set; } = new();

    public bool Confirmed { get; set; } = true;
}
