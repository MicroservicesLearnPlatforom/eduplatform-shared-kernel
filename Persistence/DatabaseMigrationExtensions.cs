using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EduPlatform.SharedKernel.Persistence;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Switches a service from EnsureCreated() to migrations without touching existing data.
    /// Databases created by EnsureCreated() have all tables but no (or an outdated) __EFMigrationsHistory,
    /// so Migrate() would try to create every table again ("There is already an object named ...").
    /// If <paramref name="probeTable"/> exists but <paramref name="baselineMigrationId"/> is not recorded,
    /// the baseline is marked as applied first; then all newer migrations run normally.
    /// Empty/new databases are created by the migrations as usual.
    /// </summary>
    /// <param name="baselineMigrationId">Full id of the baseline migration, e.g. "20261115120000_InitialBaseline".</param>
    /// <param name="probeTable">A table that the baseline creates and that every existing database has, e.g. "Courses".</param>
    /// <remarks>Both values are compile-time constants of the calling service (never user input).</remarks>
    public static async Task MigrateWithBaselineAsync(
        this DatabaseFacade database,
        string baselineMigrationId,
        string probeTable,
        CancellationToken ct = default)
    {
        if (await database.CanConnectAsync(ct))
        {
            var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "8.0.0";

#pragma warning disable EF1002 // identifiers are constants from the calling service, see remarks
            await database.ExecuteSqlRawAsync($"""
                IF OBJECT_ID(N'[dbo].[{probeTable}]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
                        CREATE TABLE [dbo].[__EFMigrationsHistory] (
                            [MigrationId] nvarchar(150) NOT NULL,
                            [ProductVersion] nvarchar(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId]));

                    IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'{baselineMigrationId}')
                        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES (N'{baselineMigrationId}', N'{productVersion}');
                END
                """, ct);
#pragma warning restore EF1002
        }

        await database.MigrateAsync(ct);
    }
}
