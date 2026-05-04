using System.Text;
using ServerDbApp.Data;
using ServerDbApp.Models;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "servers.db";
string serversCsv = Path.Combine(AppContext.BaseDirectory, "Csv", "servers.csv");
string databasesCsv = Path.Combine(AppContext.BaseDirectory, "Csv", "databases.csv");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(serversCsv, databasesCsv);

Console.WriteLine();

string choice;
do
{
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║   УПРАВЛЕНИЕ БАЗАМИ ДАННЫХ         ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║ 1 — Показать все серверы           ║");
    Console.WriteLine("║ 2 — Показать все базы данных       ║");
    Console.WriteLine("║ 3 — Добавить базу данных           ║");
    Console.WriteLine("║ 4 — Редактировать базу данных      ║");
    Console.WriteLine("║ 5 — Удалить базу данных            ║");
    Console.WriteLine("║ 6 — Отчёты                         ║");
    Console.WriteLine("║ 7 — Фильтр по серверу              ║");
    Console.WriteLine("║ 0 — Выход                          ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");

    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowServers(db); break;
        case "2": ShowDatabases(db); break;
        case "3": AddDatabase(db); break;
        case "4": EditDatabase(db); break;
        case "5": DeleteDatabase(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByServer(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }
    Console.WriteLine();
} while (choice != "0");

static void ShowServers(DatabaseManager db)
{
    Console.WriteLine("---- Все серверы ----");
    var servers = db.GetAllServers();
    foreach (var server in servers)
        Console.WriteLine("  " + server);
    Console.WriteLine($"Итого: {servers.Count}");
}

static void ShowDatabases(DatabaseManager db)
{
    Console.WriteLine("---- Все базы данных ----");
    var databases = db.GetAllDatabases();
    foreach (var database in databases)
        Console.WriteLine("  " + database);
    Console.WriteLine($"Итого: {databases.Count}");
}

static void AddDatabase(DatabaseManager db)
{
    Console.WriteLine("---- Добавление базы данных ----");
    Console.WriteLine("Доступные серверы:");
    var servers = db.GetAllServers();
    foreach (var server in servers)
        Console.WriteLine("  " + server);

    Console.Write("ID сервера: ");
    if (!int.TryParse(Console.ReadLine(), out int serverId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    Console.Write("Название базы данных: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: название не может быть пустым.");
        return;
    }

    Console.Write("Объём данных (ГБ): ");
    if (!int.TryParse(Console.ReadLine(), out int sizeGb))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    try
    {
        var database = new Database(0, serverId, name, sizeGb);
        db.AddDatabase(database);
        Console.WriteLine("База данных добавлена.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditDatabase(DatabaseManager db)
{
    Console.WriteLine("---- Редактирование базы данных ----");
    Console.Write("Введите ID базы данных: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var database = db.GetDatabaseById(id);
    if (database == null)
    {
        Console.WriteLine($"База данных с ID={id} не найдена.");
        return;
    }

    Console.WriteLine($"Текущие данные: {database}");
    Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");

    Console.Write($"Название [{database.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0)
        database.Name = input;

    Console.Write($"ID сервера [{database.ServerId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newServerId))
        database.ServerId = newServerId;

    Console.Write($"Объём данных (ГБ) [{database.SizeGb}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newSizeGb))
    {
        try
        {
            database.SizeGb = newSizeGb;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }

    db.UpdateDatabase(database);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteDatabase(DatabaseManager db)
{
    Console.WriteLine("---- Удаление базы данных ----");
    Console.Write("Введите ID базы данных: ");

    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var database = db.GetDatabaseById(id);
    if (database == null)
    {
        Console.WriteLine($"База данных с ID={id} не найдена.");
        return;
    }

    Console.Write($"Удалить «{database.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeleteDatabase(id);
        Console.WriteLine("База данных удалена.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 - Базы данных по серверам");
        Console.WriteLine(" 2 - Количество баз данных на серверах");
        Console.WriteLine(" 3 - Средний объём баз данных по серверам");
        Console.WriteLine(" 0 - Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? "";

        switch (choice)
        {
            case "1": Report1_DatabasesWithServers(db); break;
            case "2": Report2_CountByServer(db); break;
            case "3": Report3_AvgSizeByServer(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }
        Console.WriteLine();
    } while (choice != "0");
}

static void Report1_DatabasesWithServers(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT d.db_name, s.server_name, d.size_gb
                 FROM database_info d
                 JOIN server s ON d.server_id = s.server_id
                 ORDER BY d.db_name")
        .Title("Базы данных по серверам")
        .Header("База данных", "Сервер", "Объём (ГБ)")
        .ColumnWidths(25, 25, 15)
        .Numbered()
        .Print();
}

static void Report2_CountByServer(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT s.server_name, COUNT(*) AS cnt
                 FROM database_info d
                 JOIN server s ON d.server_id = s.server_id
                 GROUP BY s.server_name
                 ORDER BY s.server_name")
        .Title("Количество баз данных на серверах")
        .Header("Сервер", "Количество БД")
        .ColumnWidths(30, 15)
        .Numbered()
        .Print();
}

static void Report3_AvgSizeByServer(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT s.server_name, ROUND(AVG(d.size_gb), 1) AS avg_size
                 FROM database_info d
                 JOIN server s ON d.server_id = s.server_id
                 GROUP BY s.server_name
                 ORDER BY avg_size DESC")
        .Title("Средний объём баз данных по серверам")
        .Header("Сервер", "Средний объём (ГБ)")
        .ColumnWidths(30, 20)
        .Numbered()
        .Print();
}

static void FilterByServer(DatabaseManager db)
{
    Console.WriteLine("---- Фильтр по серверу ----");
    Console.WriteLine("Доступные серверы:");
    var servers = db.GetAllServers();
    foreach (var server in servers)
        Console.WriteLine("  " + server);

    Console.Write("Введите ID сервера: ");
    if (!int.TryParse(Console.ReadLine(), out int serverId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var databases = db.GetDatabasesByServer(serverId);
    if (databases.Count == 0)
    {
        Console.WriteLine("На этом сервере нет баз данных.");
        return;
    }

    Console.WriteLine($"\nБазы данных сервера #{serverId}:");
    foreach (var database in databases)
        Console.WriteLine("  " + database);
    Console.WriteLine($"Итого: {databases.Count}");
}