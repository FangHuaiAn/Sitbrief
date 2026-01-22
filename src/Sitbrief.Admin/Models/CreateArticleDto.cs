using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class CreateArticleDto
{
    [Required(ErrorMessage = "標題為必填")]
    [MaxLength(500, ErrorMessage = "標題長度不能超過 500 字元")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "摘要為必填")]
    [MaxLength(2000, ErrorMessage = "摘要長度不能超過 2000 字元")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "來源網址為必填")]
    [Url(ErrorMessage = "請輸入有效的網址")]
    [MaxLength(1000)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "媒體/智庫名稱為必填")]
    [MaxLength(200)]
    public string SourceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "來源類型為必填")]
    public int SourceType { get; set; }

    [Required(ErrorMessage = "發布日期為必填")]
    public DateTime PublishedDate { get; set; } = DateTime.Now;

    [MaxLength(50000)]
    public string? Content { get; set; }
}
