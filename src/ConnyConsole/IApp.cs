namespace ConnyConsole;

public interface IApp
{
    Task<int> RunAsync(string[] arguments);
}
