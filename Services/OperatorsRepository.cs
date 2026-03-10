using System.Data;
using BMSBTRwp_API.Models;
using Microsoft.Data.SqlClient;

namespace BMSBTRwp_API.Services;

/// <summary>
/// ADO.NET-based CRUD operations for dbo.Operators.
/// Uses DbTextFileService to validate the table definition from db.txt.
/// </summary>
public class OperatorsRepository
{
    private const string TableName = "Operators";

    private readonly string _connectionString;
    private readonly DbTextFileService _dbTextFileService;
    private readonly ILogger<OperatorsRepository> _logger;

    public OperatorsRepository(
        IConfiguration configuration,
        DbTextFileService dbTextFileService,
        ILogger<OperatorsRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("ConnectionBMSBT")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionBMSBT' not found. " +
                "Add it to appsettings.json under ConnectionStrings.");

        _dbTextFileService = dbTextFileService;
        _logger = logger;
    }

    private TableInfo GetOperatorsTableInfo()
    {
        var table = _dbTextFileService.FindTable(TableName);
        if (table is null)
        {
            throw new InvalidOperationException($"Table '{TableName}' not defined in db.txt.");
        }

        return table;
    }

    public async Task<List<Operator>> GetAllAsync()
    {
        GetOperatorsTableInfo(); // ensures table exists in db.txt

        var list = new List<Operator>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            SELECT Id, Name, Project, Email, Username, Password, Role, IsActive
            FROM dbo.Operators
            """;

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

        while (await reader.ReadAsync())
        {
            list.Add(MapOperator(reader));
        }

        return list;
    }

    public async Task<Operator?> GetByIdAsync(int id)
    {
        GetOperatorsTableInfo();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            SELECT Id, Name, Project, Email, Username, Password, Role, IsActive
            FROM dbo.Operators
            WHERE Id = @Id
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return MapOperator(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Operator op)
    {
        GetOperatorsTableInfo();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            INSERT INTO dbo.Operators (Name, Project, Email, Username, Password, Role, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Project, @Email, @Username, @Password, @Role, @IsActive)
            """;

        await using var cmd = new SqlCommand(sql, conn);
        AddParameters(cmd, op, includeId: false);

        var insertedId = (int)(await cmd.ExecuteScalarAsync())!;
        _logger.LogInformation("Created Operator with Id {Id}", insertedId);
        return insertedId;
    }

    public async Task<bool> UpdateAsync(int id, Operator op)
    {
        GetOperatorsTableInfo();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            UPDATE dbo.Operators
            SET Name = @Name,
                Project = @Project,
                Email = @Email,
                Username = @Username,
                Password = @Password,
                Role = @Role,
                IsActive = @IsActive
            WHERE Id = @Id
            """;

        await using var cmd = new SqlCommand(sql, conn);
        AddParameters(cmd, op, includeId: true, idOverride: id);

        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        GetOperatorsTableInfo();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "DELETE FROM dbo.Operators WHERE Id = @Id";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    private static Operator MapOperator(SqlDataReader reader)
    {
        return new Operator
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Project = reader["Project"] as string,
            Email = reader["Email"] as string,
            Username = reader["Username"] as string,
            Password = reader["Password"] as string,
            Role = reader["Role"] as string,
            IsActive = reader["IsActive"] is DBNull ? null : reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    private static void AddParameters(SqlCommand cmd, Operator op, bool includeId, int? idOverride = null)
    {
        if (includeId)
        {
            cmd.Parameters.AddWithValue("@Id", idOverride ?? op.Id);
        }

        cmd.Parameters.AddWithValue("@Name", op.Name);
        cmd.Parameters.AddWithValue("@Project", (object?)op.Project ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)op.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Username", (object?)op.Username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Password", (object?)op.Password ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Role", (object?)op.Role ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", (object?)op.IsActive ?? DBNull.Value);
    }
}

