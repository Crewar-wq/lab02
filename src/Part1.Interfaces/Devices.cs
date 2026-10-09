namespace Lab02.Interfaces;

public interface IPrinter { void Print(string document); }
public interface IScanner { string Scan(); }
public interface IFax { void SendFax(string document, string number); }

public sealed class Printer : IPrinter
{
    public void Print(string document) =>
        Console.WriteLine($"Printer: {document}");
}
public sealed class Scanner : IScanner
{
    public string Scan() => "Scanned document";
}
public sealed class MultifunctionDevice : IPrinter, IScanner, IFax
{
    public void Print(string document) =>
        Console.WriteLine($"MFD print: {document}");
    public string Scan() => "MFD scanned document";
    public void SendFax(string document, string number) =>
        Console.WriteLine($"MFD fax to {number}: {document}");
}
