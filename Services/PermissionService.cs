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
        // SELECT user_has_permission(@p_user_id, @p_perm_code);
        var sql = @"SELECT user_has_permission({0}, {1});";

        // ExecuteScalar preko EF-a (jedna vrijednost)
        var result = await _db.Database
            .SqlQueryRaw<bool>(sql, userId, permissionCode)
            .SingleAsync(ct);

        return result;
    }
}