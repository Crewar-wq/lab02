namespace Lab02.Data;

public interface IEntity { int Id { get; set; } }
public sealed class Author : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Book> Books { get; set; } = [];
}
public sealed class Book : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
