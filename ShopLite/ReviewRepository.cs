using LiteDB;

namespace ShopLite;

public class ReviewRepository : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<Review> _reviews;

    public ReviewRepository(string path)
    {
        _db = new LiteDatabase(path);
        _reviews = _db.GetCollection<Review>("reviews");
        _reviews.EnsureIndex(r => r.ProductId);
    }

    public int Add(Review review) => _reviews.Insert(review).AsInt32;

    public List<Review> GetByProduct(int productId) =>
        _reviews.Find(r => r.ProductId == productId).ToList();

    public List<Review> FindByTag(string tag) =>
        _reviews.Query().Where(r => r.Tags.Contains(tag)).ToList();

    public List<Review> GetAll() => _reviews.FindAll().ToList();

    public bool Delete(int id) => _reviews.Delete(id);

    public void Dispose() => _db.Dispose();
}
