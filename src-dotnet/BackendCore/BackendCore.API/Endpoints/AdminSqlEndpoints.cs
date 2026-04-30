using System.Data;
using System.Text.RegularExpressions;
using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class AdminSqlEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/sql/execute", Execute);
    }

    private static async Task<IResult> Execute(
        ExecuteSqlRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var sql = request.Sql?.Trim();
        if (string.IsNullOrWhiteSpace(sql))
        {
            return Results.BadRequest(new { message = "SQL-запрос пуст." });
        }

        // Разрешаем завершающую ';' и удаляем её из конца запроса.
        sql = Regex.Replace(sql, @"\s*;\s*$", "");
        if (string.IsNullOrWhiteSpace(sql))
        {
            return Results.BadRequest(new { message = "SQL-запрос пуст после удаления ';'." });
        }

        if (!IsAllowedSql(sql, out var reason))
        {
            return Results.BadRequest(new { message = reason ?? "Недопустимый SQL-запрос." });
        }

        // Если пользователь вставил несколько операторов — запрещаем (т.к. 'Execute' ожидает 1 оператор).
        if (sql.Contains(";"))
        {
            return Results.BadRequest(new { message = "Разрешён только один SQL-оператор." });
        }

        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 30;

        // Для SELECT возвращаем колонки и строки (JSON-удобный формат).
        if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            await using var reader = await command.ExecuteReaderAsync(ct);
            var columns = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>();
                foreach (var col in columns)
                {
                    var ordinal = reader.GetOrdinal(col);
                    row[col] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                }

                rows.Add(row);
            }

            return Results.Ok(
                new
                {
                    columns,
                    rows,
                    rowsCount = rows.Count,
                }
            );
        }

        // Для INSERT/UPDATE/DELETE возвращаем только количество затронутых строк.
        var affected = await command.ExecuteNonQueryAsync(ct);
        return Results.Ok(new { rowsAffected = affected });
    }

    private static bool IsAllowedSql(string sql, out string? reason)
    {
        reason = null;
        // Разрешаем только один оператор и только базовые команды.
        var match = Regex.Match(sql, @"^\s*(\w+)", RegexOptions.IgnoreCase);
        var first = match.Success ? match.Groups[1].Value : string.Empty;

        var allowed =
            string.Equals(first, "SELECT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(first, "INSERT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(first, "UPDATE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(first, "DELETE", StringComparison.OrdinalIgnoreCase);

        if (!allowed)
        {
            reason = "Разрешены только SELECT, INSERT, UPDATE, DELETE.";
            return false;
        }

        return true;
    }
}

