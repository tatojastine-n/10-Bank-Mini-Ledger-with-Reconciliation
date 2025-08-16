using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum TransactionType { Deposit, Withdrawal }

public class Transaction
{
    public DateTime Date { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public string Memo { get; }

    public Transaction(DateTime date, TransactionType type, decimal amount, string memo)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        Date = date;
        Type = type;
        Amount = amount;
        Memo = memo;
    }

    public decimal SignedAmount => Type == TransactionType.Deposit ? Amount : -Amount;

    public override string ToString()
    {
        return $"{Date:d} {Type}: {Amount:C} - {Memo}";
    }
}
public class BankAccount
{
    private decimal _balance;
    private readonly List<Transaction> _transactions = new List<Transaction>();
    private readonly decimal _overdraftThreshold;
    private readonly decimal _suspiciousAmountThreshold;

    public BankAccount(decimal openingBalance, decimal overdraftThreshold = 0, decimal suspiciousAmountThreshold = 10000)
    {
        _balance = openingBalance;
        _overdraftThreshold = overdraftThreshold;
        _suspiciousAmountThreshold = suspiciousAmountThreshold;
    }

    public void ProcessTransaction(Transaction transaction)
    {
        if (transaction.Type == TransactionType.Withdrawal &&
            (_balance - transaction.Amount) < _overdraftThreshold)
        {
            throw new InvalidOperationException(
                $"Overdraft prevented! Available: {_balance:C}, Attempted withdrawal: {transaction.Amount:C}");
        }

        _transactions.Add(transaction);
        _balance += transaction.SignedAmount;
    }

    public (decimal Variance, List<string> SuspiciousTransactions) Reconcile(decimal statementEndingBalance)
    {
        var variance = _balance - statementEndingBalance;
        var suspiciousTransactions = new List<string>();

        foreach (var t in _transactions)
        {
            if (t.Amount > _suspiciousAmountThreshold)
            {
                suspiciousTransactions.Add(
                    $"Suspicious transaction: {t} (Amount exceeds {_suspiciousAmountThreshold:C} threshold)");
            }
        }

        return (variance, suspiciousTransactions);
    }

    public void PrintStatement()
    {
        Console.WriteLine("ACCOUNT STATEMENT");

        decimal runningBalance = _balance - _transactions.Sum(t => t.SignedAmount);
        Console.WriteLine($"Opening Balance: {runningBalance:C}");

        foreach (var t in _transactions.OrderBy(t => t.Date))
        {
            runningBalance += t.SignedAmount;
            Console.WriteLine($"{t.Date:d} {t.Type} {t.Amount:C} - {t.Memo} | Balance: {runningBalance:C}");
        }

        Console.WriteLine($"\nClosing Balance: {runningBalance:C}");
    }
}


namespace Bank_Mini_Ledger_with_Reconciliation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
 
                var account = new BankAccount(1000m, overdraftThreshold: -500m, suspiciousAmountThreshold: 5000m);

                account.ProcessTransaction(new Transaction(
                    DateTime.Parse("2023-10-01"),
                    TransactionType.Deposit,
                    2000m,
                    "Paycheck"));

                account.ProcessTransaction(new Transaction(
                    DateTime.Parse("2023-10-03"),
                    TransactionType.Withdrawal,
                    500m,
                    "Rent payment"));

                account.PrintStatement();

                var (variance, suspicious) = account.Reconcile(2400m);

                Console.WriteLine("\nRECONCILIATION REPORT");
                Console.WriteLine($"Variance: {variance:C}");

                if (variance != 0)
                {
                    Console.WriteLine("Warning: Account balance doesn't match bank statement");
                }

                if (suspicious.Any())
                {
                    Console.WriteLine("\nSUSPICIOUS TRANSACTIONS:");
                    foreach (var s in suspicious)
                    {
                        Console.WriteLine($"- {s}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
