namespace Lab02.Interfaces;

public interface IPayable { void Pay(decimal amount); }
public sealed class CreditCard : IPayable
{
    public void Pay(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Console.WriteLine($"Credit card payment: {amount:F2}");
    }
}
public sealed class Cash : IPayable
{
    public void Pay(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Console.WriteLine($"Cash payment: {amount:F2}");
    }
}

public interface ILogger { void Log(string message); }
public sealed class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine($"LOG: {message}");
}
public sealed class FileLogger : ILogger
{
    private readonly string _path;
    public FileLogger(string path)
    {
        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }
    public void Log(string message) =>
        File.AppendAllText(_path, message + Environment.NewLine);
}

public static class Demo
{
    public static void DrawAll(List<IDrawable> shapes)
    {
        foreach (var shape in shapes) shape.Draw();
    }
    public static void PrintShapeInfo(IShape shape) =>
        Console.WriteLine($"{shape.GetType().Name}: area={shape.GetArea():F2}, " +
            $"perimeter={shape.GetPerimeter():F2}");
    public static void ProcessPayment(IPayable method, decimal amount) =>
        method.Pay(amount);
    public static void DoWork(ILogger logger)
    {
        logger.Log("Work started");
        logger.Log("Work completed");
    }
}
