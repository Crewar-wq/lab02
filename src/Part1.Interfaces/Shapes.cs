namespace Lab02.Interfaces;

public interface IMovable { void Move(int x, int y); }
public interface IDrawable { void Draw(); }
public interface IShape
{
    double GetArea();
    double GetPerimeter();
}
public interface I3DShape : IShape { double GetVolume(); }

public sealed class Point(int x, int y) : IMovable
{
    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;
    // Move sets the new absolute coordinates.
    public void Move(int x, int y) { X = x; Y = y; }
    public override string ToString() => $"Point({X}, {Y})";
}

public sealed class Circle : IDrawable, IShape
{
    public double Radius { get; }
    public Circle(double radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        if (!double.IsFinite(radius))
            throw new ArgumentOutOfRangeException(nameof(radius));
        Radius = radius;
    }
    public void Draw() => Console.WriteLine($"Circle: radius={Radius}");
    public double GetArea() => Math.PI * Radius * Radius;
    public double GetPerimeter() => 2 * Math.PI * Radius;
}

public sealed class Rectangle : IDrawable, IShape
{
    public double Width { get; }
    public double Height { get; }
    public Rectangle(double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (!double.IsFinite(width) || !double.IsFinite(height))
            throw new ArgumentOutOfRangeException(nameof(width));
        Width = width;
        Height = height;
    }
    public void Draw() => Console.WriteLine($"Rectangle: {Width} x {Height}");
    public double GetArea() => Width * Height;
    public double GetPerimeter() => 2 * (Width + Height);
}

public sealed class Cube : I3DShape
{
    public double Edge { get; }
    public Cube(double edge)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(edge);
        if (!double.IsFinite(edge))
            throw new ArgumentOutOfRangeException(nameof(edge));
        Edge = edge;
    }
    public double GetArea() => 6 * Edge * Edge;
    // For a cube, perimeter means the sum of all twelve edges.
    public double GetPerimeter() => 12 * Edge;
    public double GetVolume() => Edge * Edge * Edge;
}
