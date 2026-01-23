using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly SitbriefDbContext _context;

    public ArticleRepository(SitbriefDbContext context)
    {
        _context = context;
    }

    public async Task<Article?> GetByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await _context.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<Article> AddAsync(Article article)
    {
        article.CreatedDate = DateTime.UtcNow;
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task UpdateAsync(Article article)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article != null)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Articles.AnyAsync(a => a.Id == id);
    }

    public async Task LinkTopicsAsync(int articleId, IEnumerable<int> topicIds, bool confirmed = true)
    {
        // Remove existing links
        var existingLinks = _context.ArticleTopics.Where(at => at.ArticleId == articleId);
        _context.ArticleTopics.RemoveRange(existingLinks);

        // Add new links
        foreach (var topicId in topicIds)
        {
            _context.ArticleTopics.Add(new ArticleTopic
            {
                ArticleId = articleId,
                TopicId = topicId,
                Confidence = 1.0,
                IsConfirmed = confirmed,
                AddedDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
