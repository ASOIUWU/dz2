using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;
using ServerDbApp.Models;

namespace ServerDbApp.Data;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public void InitializeDatabase(string serversCsvPath, string databasesCsvPath)
    {
        CreateTables();

        if (GetAllServers().Count == 0 && File.Exists(serversCsvPath))
        {
            ImportServersFromCsv(serversCsvPath);
            Console.WriteLine($"[OK] Загружены серверы из {serversCsvPath}");
        }

        if (GetAllDatabases().Count == 0 && File.Exists(databasesCsvPath))
        {
            ImportDatabasesFromCsv(databasesCsvPath);
            Console.WriteLine($"[OK] Загружены базы данных из {databasesCsvPath}");
        }
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS server (
                server_id INTEGER PRIMARY KEY AUTOINCREMENT,
                server_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS database_info (
                db_id INTEGER PRIMARY KEY AUTOINCREMENT,
                server_id INTEGER NOT NULL,
                db_name TEXT NOT NULL,
                size_gb INTEGER NOT NULL,
                FOREIGN KEY (server_id) REFERENCES server(server_id)
            );";
        cmd.ExecuteNonQuery();
    }

    private void ImportServersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO server (server_id, server_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    private void ImportDatabasesFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO database_info (db_id, server_id, db_name, size_gb)
                VALUES (@id, @serverId, @name, @size)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@serverId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@size", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }

    public List<Server> GetAllServers()
    {
        var result = new List<Server>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT server_id, server_name FROM server ORDER BY server_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Server(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Database> GetAllDatabases()
    {
        var result = new List<Database>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT db_id, server_id, db_name, size_gb FROM database_info ORDER BY db_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Database(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }
        return result;
    }

    public Database? GetDatabaseById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT db_id, server_id, db_name, size_gb FROM database_info WHERE db_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Database(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3));
        }
        return null;
    }

    public void AddDatabase(Database database)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO database_info (server_id, db_name, size_gb)
            VALUES (@serverId, @name, @size)";
        cmd.Parameters.AddWithValue("@serverId", database.ServerId);
        cmd.Parameters.AddWithValue("@name", database.Name);
        cmd.Parameters.AddWithValue("@size", database.SizeGb);
        cmd.ExecuteNonQuery();
    }

    public void UpdateDatabase(Database database)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE database_info
            SET server_id = @serverId, db_name = @name, size_gb = @size
            WHERE db_id = @id";
        cmd.Parameters.AddWithValue("@id", database.Id);
        cmd.Parameters.AddWithValue("@serverId", database.ServerId);
        cmd.Parameters.AddWithValue("@name", database.Name);
        cmd.Parameters.AddWithValue("@size", database.SizeGb);
        cmd.ExecuteNonQuery();
    }

    public void DeleteDatabase(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM database_info WHERE db_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public List<Database> GetDatabasesByServer(int serverId)
    {
        var result = new List<Database>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT db_id, server_id, db_name, size_gb
            FROM database_info WHERE server_id = @serverId ORDER BY db_name";
        cmd.Parameters.AddWithValue("@serverId", serverId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Database(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }
        return result;
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }
}