using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace NorthWind.Sales.Backend.Controllers.System;

public static class ErrorReportsEndpoints
{
    public static WebApplication UseErrorReportsEndpoints(this WebApplication app)
    {
        app.MapPost("/system/error-reports", async (HttpContext ctx, IConfiguration config) =>
        {
            try
            {
                var body = await ctx.Request.ReadFromJsonAsync<ErrorReportDto>() ?? new ErrorReportDto();
                var connStr = config.GetSection("DBOptions")["DomainLogsConnectionString"]
                              ?? config.GetSection("DBOptions")["ErrorLogConnectionString"]
                              ?? config.GetConnectionString("ErrorLogConnection")
                              ?? string.Empty;
                if (string.IsNullOrWhiteSpace(connStr)) return Results.Problem("No error log connection string configured.", statusCode: 500);

                using var con = new SqlConnection(connStr);
                await con.OpenAsync();
                await EnsureTableExistsAsync(con);
                using var cmd = new SqlCommand(@"INSERT INTO dbo.ErrorReports
                    (Message, StackTrace, Url, UserAgent, LineNumber, ColumnNumber, Component, Extra, CreatedAt)
                    VALUES (@Message, @StackTrace, @Url, @UserAgent, @LineNumber, @ColumnNumber, @Component, @Extra, SYSUTCDATETIME());", con);
                cmd.Parameters.Add(new("@Message", SqlDbType.NVarChar, -1) { Value = (object?)body.message ?? DBNull.Value });
                cmd.Parameters.Add(new("@StackTrace", SqlDbType.NVarChar, -1) { Value = (object?)body.stack ?? DBNull.Value });
                cmd.Parameters.Add(new("@Url", SqlDbType.NVarChar, 1024) { Value = (object?)body.url ?? DBNull.Value });
                cmd.Parameters.Add(new("@UserAgent", SqlDbType.NVarChar, 512) { Value = (object?)body.userAgent ?? DBNull.Value });
                cmd.Parameters.Add(new("@LineNumber", SqlDbType.Int) { Value = body.line ?? (object)DBNull.Value });
                cmd.Parameters.Add(new("@ColumnNumber", SqlDbType.Int) { Value = body.column ?? (object)DBNull.Value });
                cmd.Parameters.Add(new("@Component", SqlDbType.NVarChar, 256) { Value = (object?)body.component ?? DBNull.Value });
                cmd.Parameters.Add(new("@Extra", SqlDbType.NVarChar, -1) { Value = (object?)body.extra ?? DBNull.Value });
                await cmd.ExecuteNonQueryAsync();
                return Results.Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapGet("/system/error-reports", async (HttpContext ctx, IConfiguration config, int page = 1, int pageSize = 50) =>
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 200);
                var offset = (page - 1) * pageSize;
                var connStr = config.GetSection("DBOptions")["DomainLogsConnectionString"]
                              ?? config.GetSection("DBOptions")["ErrorLogConnectionString"]
                              ?? config.GetConnectionString("ErrorLogConnection")
                              ?? string.Empty;
                if (string.IsNullOrWhiteSpace(connStr)) return Results.Problem("No error log connection string configured.", statusCode: 500);

                var items = new List<ErrorReportRow>();
                int total = 0;
                await using (var con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();
                    await EnsureTableExistsAsync(con);
                    // total
                    await using (var cmdCount = new SqlCommand("SELECT COUNT(1) FROM dbo.ErrorReports", con))
                    {
                        total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    }
                    // page
                    await using (var cmd = new SqlCommand(@"SELECT Id, Message, Url, CreatedAt
                                FROM dbo.ErrorReports
                                ORDER BY Id DESC
                                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;", con))
                    {
                        cmd.Parameters.AddWithValue("@Offset", offset);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);
                        await using var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            items.Add(new ErrorReportRow(
                                reader.GetInt32(0),
                                reader.IsDBNull(1) ? null : reader.GetString(1),
                                reader.IsDBNull(2) ? null : reader.GetString(2),
                                reader.GetDateTime(3)
                            ));
                        }
                    }
                }
                return Results.Ok(new { total, items });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        return app;
    }

    private record ErrorReportDto(string? message = null, string? stack = null, string? url = null,
        string? userAgent = null, int? line = null, int? column = null, string? component = null, string? extra = null);
    private record ErrorReportRow(int id, string? message, string? url, DateTime createdAt);

    private static async Task EnsureTableExistsAsync(SqlConnection con)
    {
        var sql = @"IF OBJECT_ID('dbo.ErrorReports','U') IS NULL
BEGIN
  CREATE TABLE dbo.ErrorReports (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Message       NVARCHAR(MAX)     NULL,
    StackTrace    NVARCHAR(MAX)     NULL,
    Url           NVARCHAR(1024)    NULL,
    UserAgent     NVARCHAR(512)     NULL,
    LineNumber    INT               NULL,
    ColumnNumber  INT               NULL,
    Component     NVARCHAR(256)     NULL,
    Extra         NVARCHAR(MAX)     NULL,
    CreatedAt     DATETIME2(3)      NOT NULL CONSTRAINT DF_ErrorReports_CreatedAt DEFAULT SYSUTCDATETIME()
  );
  CREATE INDEX IX_ErrorReports_CreatedAt ON dbo.ErrorReports (CreatedAt DESC);
END";
        await using var cmd = new SqlCommand(sql, con);
        await cmd.ExecuteNonQueryAsync();
    }
}
