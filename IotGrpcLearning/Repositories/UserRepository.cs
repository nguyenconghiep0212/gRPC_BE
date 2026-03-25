using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Repositories;

/// <summary>
/// Repository for User data access.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ISqliteConnectionFactory _dbFactory;

    public UserRepository(ISqliteConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<UserDto?> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default)
    {


        return null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, employee_id, username, password_hash, password_salt, is_active, last_login, created_at, updated_at
            FROM Users 
            WHERE username = @username
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@email", username.ToLowerInvariant());

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToUserDto(reader);
        }

        return null;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, employee_id, username, password_hash, password_salt, is_active, last_login, created_at, updated_at
            FROM Users 
            WHERE id = @id
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToUserDto(reader);
        }

        return null;
    }

    public async Task<UserDto> CreateAsync(UserDto user, CancellationToken ct = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (employee_id, username, password_hash, password_salt, is_active, last_login, created_at, updated_at) 
            VALUES (@employee_id, @username, @password_hash, @password_salt, @is_active, @last_login, @created_at, @updated_at);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@employee_id", user.EmployeeId);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@password_salt", user.PasswordSalt);
        cmd.Parameters.AddWithValue("@is_active", user.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@last_login", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow.ToString("o"));

        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return user with { Id = newId, CreatedAt = DateTime.UtcNow };
    }

    public async Task<bool> UpdateAsync(int id, UserDto user, CancellationToken ct = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Users 
            SET password_hash = @password_hash, 
                password_salt = @password_salt,
                username = @username,
                is_active = @is_active
            WHERE id = @id;";

        cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@password_salt", user.PasswordSalt);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@is_active", user.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE email = @email;";
        cmd.Parameters.AddWithValue("@email", email.ToLowerInvariant());

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) > 0;
    }

    private static UserDto MapToUserDto(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);
        var employeeId = reader.GetInt32(1);
        var username = reader.GetString(2);
        var passwordHash = reader.GetString(3);
        var passwordSalt = reader.GetString(4);
        var isActive = reader.GetInt32(5) == 1;
        var lastLogin = DateTime.Parse(reader.GetString(6));
        var createdAt = DateTime.Parse(reader.GetString(7));
        var updatedAt = DateTime.Parse(reader.GetString(8));

        return new UserDto(id, employeeId, username, passwordHash, passwordSalt, isActive, lastLogin, createdAt, updatedAt);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}