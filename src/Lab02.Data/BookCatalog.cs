namespace Lab02.Data;

// Business rules depend on contracts rather than SQL or EF Core.
public sealed class BookCatalog(IUnitOfWork work)
{
    public void ChangePrice(int bookId, decimal price)
    {
        if (price < 0 || decimal.Round(price, 2) != price ||
            price > long.MaxValue / 100m)
            throw new ArgumentOutOfRangeException(nameof(price));
        var book = work.Books.GetById(bookId);
        book.Price = price;
        work.Books.Update(book);
        work.SaveChanges();
    }
}
