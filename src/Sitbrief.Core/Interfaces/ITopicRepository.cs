using Sitbrief.Core.Entities;

namespace Sitbrief.Core.Interfaces;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(int id);
    Task<IEnumerable<Topic>> GetAllAsync();
    Task<Topic?> GetWithArticlesAsync(int id);
    Task<Topic> AddAsync(Topic topic);
    Task UpdateAsync(Topic topic);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
