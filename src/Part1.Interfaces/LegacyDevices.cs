namespace Lab02.Interfaces.Legacy;

// The deliberately bad design requested in task 3.
public interface IDevice
{
    void Print();
    void Scan();
    void Fax();
}
public sealed class Printer : IDevice
{
    public void Print() { }
    public void Scan() { } // Printer cannot scan.
    public void Fax() { }  // Printer cannot send a fax.
}
public sealed class Scanner : IDevice
{
    public void Print() { } // Scanner cannot print.
    public void Scan() { }
    public void Fax() { }   // Scanner cannot send a fax.
}
