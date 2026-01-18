using AccountManager.Data;
using Microsoft.EntityFrameworkCore;

public interface IPermissionService
{
    Task<bool> HasAsync(int userId, string permissionCode, CancellationToken ct = default);
}

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _db;

    public PermissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasAsync(int userId, string permissionCode, CancellationToken ct = default)
    {
        var sql = @"SELECT user_has_permission({0}, {1});";

        var result = await _db.Database
            .SqlQueryRaw<bool>(sql, userId, permissionCode)
            .SingleAsync(ct);

        return result;
    }
}