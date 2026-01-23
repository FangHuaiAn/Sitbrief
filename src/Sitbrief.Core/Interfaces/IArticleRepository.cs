using Sitbrief.Core.Entities;

namespace Sitbrief.Core.Interfaces;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(int id);
    Task<IEnumerable<Article>> GetAllAsync();
    Task<Article> AddAsync(Article article);
    Task UpdateAsync(Article article);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task LinkTopicsAsync(int articleId, IEnumerable<int> topicIds, bool confirmed = true);
}
