using System.ComponentModel.DataAnnotations;
using Sitbrief.Core.Enums;

namespace Sitbrief.API.DTOs;

public class CreateArticleDto
{
    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Summary { get; set; }

    [Required]
    [Url]
    [MaxLength(1000)]
    public required string SourceUrl { get; set; }

    [Required]
    [MaxLength(200)]
    public required string SourceName { get; set; }

    [Required]
    public SourceType SourceType { get; set; }

    [Required]
    public DateTime PublishedDate { get; set; }

    [MaxLength(50000)]
    public string? Content { get; set; }
}
