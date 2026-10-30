namespace ShopLite;

public record Product(int Id, string Name, decimal Price);

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Author { get; set; } = "";
    public int Rating { get; set; }
    public string Text { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
