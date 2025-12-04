using QuickPulse.Explains;

namespace QuickCheckr.Tests.Models;

[CodeExample]
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}