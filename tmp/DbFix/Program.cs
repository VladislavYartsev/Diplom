using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=API2;Username=postgres;Password=2133";

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

const string sql = """
ALTER TABLE "Tasks"
ADD COLUMN IF NOT EXISTS "Deadline" timestamp with time zone NULL;
""";

await using var command = new NpgsqlCommand(sql, connection);
await command.ExecuteNonQueryAsync();

Console.WriteLine("Deadline column ensured.");
