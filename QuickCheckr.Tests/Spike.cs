using QuickFuzzr;

namespace QuickCheckr.Tests;

public class Spike
{
    [Fact(Skip = "demo")]
    public void Sketching()
    {
        var check =
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("amount", Fuzzr.Int())
            from deposit in Checkr.Act("Deposit", () => account.Withdraw(amount))
            from spec in Checkr.Spec("No overdraft", () => account.Balance >= 0)
            select Case.Closed;
        check.Run(1.Runs(), 50.ExecutionsPerRun());
    }

    public class Account
    {
        public int Balance = 0;
        public int OverdraftLimit = 0;

        public void Deposit(int amount)
        {
            Balance += amount;
            ApplyFee();
        }

        public void Withdraw(int amount)
        {
            //if (Balance - amount < 0 - OverdraftLimit) return;
            Balance -= amount;
            ApplyFee();
        }

        public void ApplyFee() { Balance--; }
    }
}