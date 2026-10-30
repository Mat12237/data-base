using Microsoft.Data.Sqlite;

namespace ShopLite;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string path)
    {
        _connectionString = $"Data Source={path}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public int Add(string name, decimal price)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Products (Name, Price) VALUES ($name, $price); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$price", (double)price);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<Product> GetAll()
    {
        var result = new List<Product>();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Price FROM Products ORDER BY Id";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(new Product(reader.GetInt32(0), reader.GetString(1), (decimal)reader.GetDouble(2)));
        return result;
    }

    public Product? Get(int id) => GetAll().FirstOrDefault(p => p.Id == id);
}
