using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Bugtrack.Api.Enums;
using Microsoft.EntityFrameworkCore;

public class SupportService : ISupportService
{
    private readonly AppDbContext db;

    public SupportService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<SupportResponse> CreateAsync(
        Guid userId,
        CreateSupportRequest data)
    {
        var author = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId);

        if (author == null)
        {
            throw new TicketOperationException(
                401,
                "Пользователь не найден.");
        }

        var message = new SupportMessage
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Text = data.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.SupportMessages.Add(message);

        var adminIds = await db.Users
            .Where(item =>
                item.SystemRole == SystemRole.Admin &&
                item.Id != userId)
            .Select(item => item.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = adminId,
                SupportMessageId = message.Id,
                Type = "SupportMessageCreated",
                Text = $"Новое обращение в поддержку от {author.Name}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }

        await db.SaveChangesAsync();

        return new SupportResponse
        {
            Id = message.Id,
            Text = message.Text,
            CreatedAt = message.CreatedAt,
            AuthorId = message.AuthorId,
            AuthorName = author.Name
        };
    }

    public async Task<SupportMessagesResponseDto> GetAsync(
        Guid userId,
        int page,
        int perPage)
    {
        var isAdmin = await db.Users.AnyAsync(item =>
            item.Id == userId &&
            item.SystemRole == SystemRole.Admin);

        var query = db.SupportMessages.AsNoTracking();

        if (!isAdmin)
        {
            query = query.Where(item => item.AuthorId == userId);
        }

        var total = await query.CountAsync();

        var totalPages = (int)Math.Ceiling((double)total / perPage);

        var messages = new List<SupportResponse>();

        if (page <= totalPages)
        {
            messages = await query
                .OrderByDescending(item => item.CreatedAt)
                .ThenBy(item => item.Id)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(item => new SupportResponse
                {
                    Id = item.Id,
                    Text = item.Text,
                    CreatedAt = item.CreatedAt,
                    AuthorId = item.AuthorId,
                    AuthorName = item.Author.Name
                })
                .ToListAsync();
        }

        return new SupportMessagesResponseDto
        {
            Pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                PerPage = messages.Count,
                TotalPages = totalPages
            },
            Messages = messages
        };
    }

    public async Task<SupportResponse> GetByIdAsync(
        Guid userId,
        Guid messageId)
    {
        var isAdmin = await db.Users.AnyAsync(item =>
            item.Id == userId &&
            item.SystemRole == SystemRole.Admin);

        var message = await db.SupportMessages
            .AsNoTracking()
            .Where(item =>
                item.Id == messageId &&
                (isAdmin || item.AuthorId == userId))
            .Select(item => new SupportResponse
            {
                Id = item.Id,
                Text = item.Text,
                CreatedAt = item.CreatedAt,
                AuthorId = item.AuthorId,
                AuthorName = item.Author.Name
            })
            .SingleOrDefaultAsync();

        if (message == null)
        {
            throw new TicketOperationException(
                404,
                "Обращение не найдено.");
        }

        return message;
    }
}