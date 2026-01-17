using AccountManager.Data;
using AccountManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountManager.Services
{
    public interface ISessionService
    {
        Task<Guid> CreateSessionAsync(int userId, string? ip, string? userAgent, CancellationToken ct = default);
        Task TouchAsync(Guid sessionId, CancellationToken ct = default);
        Task RevokeAllAsync(int userId, CancellationToken ct = default);
    }

    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _db;

        public SessionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> CreateSessionAsync(int userId, string? ip, string? userAgent, CancellationToken ct = default)
        {
            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IpAddress = ip,
                UserAgent = userAgent,
                // CreatedAt/LastSeenAt imaju default u bazi, ali nije problem i da ostanu default (Unspecified izbjegavamo)
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };

            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            return session.Id;
        }

        public async Task TouchAsync(Guid sessionId, CancellationToken ct = default)
        {
            // update last seen (UTC)
            var row = await _db.UserSessions.FirstOrDefaultAsync(x => x.Id == sessionId, ct);
            if (row == null) return;

            if (row.RevokedAt != null) return;

            row.LastSeenAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task RevokeAllAsync(int userId, CancellationToken ct = default)
        {
            // CALL public.sp_revoke_user_sessions(@p_user_id)
            await _db.Database.ExecuteSqlRawAsync(
                @"CALL public.sp_revoke_user_sessions({0});",
                new object[] { userId },
                ct);
        }
    }
}
