using System.Data;
using Dapper;
using Npgsql;
using Org.BouncyCastle.Math.EC;
using SematicKernelWeb.Classes;

namespace SematicKernelWeb.Database;

public class DbContext
{
    public IDbConnection CreateConnection()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(Config.Instance.ConnectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        return dataSource.OpenConnection();
    }

    public async Task Init()
    {
        await CreateDatabase();
        await ConfigureVectors();
    }

    private async Task CreateDatabase()
    {
        // create database if it doesn't exist
        await using var connection =
            new NpgsqlConnection(Config.Instance.ConnectionString.Replace(Config.Instance.DatabaseName, "postgres"));
        var sqlDbCount =
            $"SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE datname = '{Config.Instance.DatabaseName}');";
        var dbCount = await connection.ExecuteScalarAsync<int>(sqlDbCount);
        if (dbCount == 0)
        {
            var sql = $"CREATE DATABASE \"{Config.Instance.DatabaseName}\"";
            await connection.ExecuteAsync(sql);
        }
    }

    private async Task ConfigureVectors()
    {
        await using var connection = CreateConnection() as NpgsqlConnection;

        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await connection.ReloadTypesAsync();
    }
}