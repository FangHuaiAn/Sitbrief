using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly SitbriefDbContext _context;

    public TopicRepository(SitbriefDbContext context)
    {
        _context = context;
    }

    public async Task<Topic?> GetByIdAsync(int id)
    {
        return await _context.Topics.FindAsync(id);
    }

    public async Task<IEnumerable<Topic>> GetAllAsync()
    {
        return await _context.Topics
            .Include(t => t.ArticleTopics)
            .OrderByDescending(t => t.LastUpdatedDate)
            .ToListAsync();
    }

    public async Task<Topic?> GetWithArticlesAsync(int id)
    {
        return await _context.Topics
            .Include(t => t.ArticleTopics)
            .ThenInclude(at => at.Article)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Topic> AddAsync(Topic topic)
    {
        topic.CreatedDate = DateTime.UtcNow;
        topic.LastUpdatedDate = DateTime.UtcNow;
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task UpdateAsync(Topic topic)
    {
        topic.LastUpdatedDate = DateTime.UtcNow;
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Topics.AnyAsync(t => t.Id == id);
    }
}
