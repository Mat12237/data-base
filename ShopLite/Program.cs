using System.Globalization;
using ShopLite;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var products = new ProductRepository("shop.db");
using var reviews = new ReviewRepository("reviews.db");

if (products.GetAll().Count == 0)
{
    products.Add("Клавиатура", 4500m);
    products.Add("Мышь", 1800m);
    products.Add("Монитор", 12900m);
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Список товаров (SQLite)");
    Console.WriteLine("2. Добавить товар (SQLite)");
    Console.WriteLine("3. Добавить отзыв (LiteDB)");
    Console.WriteLine("4. Отзывы о товаре (LiteDB)");
    Console.WriteLine("5. Поиск отзывов по тегу (LiteDB)");
    Console.WriteLine("6. Удалить отзыв (LiteDB)");
    Console.WriteLine("7. Средние рейтинги (SQLite + LiteDB)");
    Console.WriteLine("0. Выход");

    switch (Ask("> "))
    {
        case "1":
            foreach (var p in products.GetAll())
                Console.WriteLine($"{p.Id}. {p.Name} - {p.Price:F2}");
            break;

        case "2":
        {
            var name = Ask("Название: ");
            var priceText = Ask("Цена: ").Replace(',', '.');
            if (name == "" || !decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                Console.WriteLine("Некорректные данные.");
                break;
            }
            Console.WriteLine($"Товар добавлен, id = {products.Add(name, price)}");
            break;
        }

        case "3":
        {
            if (!int.TryParse(Ask("Id товара: "), out var productId) || products.Get(productId) is null)
            {
                Console.WriteLine("Такого товара нет.");
                break;
            }
            var author = Ask("Автор: ");
            if (!int.TryParse(Ask("Оценка (1-5): "), out var rating) || rating is < 1 or > 5)
            {
                Console.WriteLine("Оценка должна быть от 1 до 5.");
                break;
            }
            var text = Ask("Текст: ");
            var tags = Ask("Теги через запятую: ")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToList();
            var id = reviews.Add(new Review { ProductId = productId, Author = author, Rating = rating, Text = text, Tags = tags });
            Console.WriteLine($"Отзыв добавлен, id = {id}");
            break;
        }

        case "4":
        {
            if (!int.TryParse(Ask("Id товара: "), out var productId))
                break;
            var list = reviews.GetByProduct(productId);
            if (list.Count == 0)
                Console.WriteLine("Отзывов нет.");
            list.ForEach(PrintReview);
            break;
        }

        case "5":
        {
            var list = reviews.FindByTag(Ask("Тег: ").ToLowerInvariant());
            if (list.Count == 0)
                Console.WriteLine("Ничего не найдено.");
            foreach (var r in list)
            {
                Console.WriteLine($"Товар: {products.Get(r.ProductId)?.Name ?? "?"}");
                PrintReview(r);
            }
            break;
        }

        case "6":
        {
            var ok = int.TryParse(Ask("Id отзыва: "), out var id) && reviews.Delete(id);
            Console.WriteLine(ok ? "Удалено." : "Отзыв не найден.");
            break;
        }

        case "7":
        {
            var stats = reviews.GetAll()
                .GroupBy(r => r.ProductId)
                .Select(g => (ProductId: g.Key, Avg: g.Average(x => x.Rating), Count: g.Count()))
                .OrderByDescending(s => s.Avg);
            var any = false;
            foreach (var s in stats)
            {
                any = true;
                Console.WriteLine($"{products.Get(s.ProductId)?.Name ?? "?"}: {s.Avg:F2} (отзывов: {s.Count})");
            }
            if (!any)
                Console.WriteLine("Отзывов пока нет.");
            break;
        }

        case "0":
            return;

        default:
            Console.WriteLine("Неизвестная команда.");
            break;
    }
}

static string Ask(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim() ?? "";
}

static void PrintReview(Review r)
{
    Console.WriteLine($"[{r.Id}] {r.Author}, {r.Rating}/5, {r.CreatedAt:dd.MM.yyyy}");
    Console.WriteLine($"  {r.Text}");
    if (r.Tags.Count > 0)
        Console.WriteLine($"  теги: {string.Join(", ", r.Tags)}");
}
