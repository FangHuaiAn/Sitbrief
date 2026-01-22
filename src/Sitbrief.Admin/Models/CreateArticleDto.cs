using System.ComponentModel.DataAnnotations;
using Sitbrief.Core.Enums;

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
    [MaxLength(1000, ErrorMessage = "來源網址長度不能超過 1000 字元")]
    public string SourceUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "媒體/智庫名稱為必填")]
    [MaxLength(200, ErrorMessage = "媒體/智庫名稱長度不能超過 200 字元")]
    public string SourceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "來源類型為必填")]
    public SourceType SourceType { get; set; }

    [Required(ErrorMessage = "發布日期為必填")]
    public DateTime PublishedDate { get; set; } = DateTime.Now;

    [MaxLength(50000)]
    public string? Content { get; set; }
}
