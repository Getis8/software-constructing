using System;

public abstract record LeaveRequest
{
    private LeaveRequest() { }

    public sealed record Approved(DateOnly StartDate, int TotalDays) : LeaveRequest;
    public sealed record Pending(string ReviewerName, bool IsUrgent) : LeaveRequest;
    public sealed record Denied(string Reason) : LeaveRequest;
}

public static class Program
{
    public static void Main()
    {
        // Оригінали
        var approved = new LeaveRequest.Approved(new DateOnly(2026, 11, 1), 14);
        var pending = new LeaveRequest.Pending("Іваненко І.І.", false);
        var denied = new LeaveRequest.Denied("Недостатньо днів відпустки");

        Console.WriteLine("=== Оригінали ===");
        Console.WriteLine(approved);
        Console.WriteLine(pending);
        Console.WriteLine(denied);

        // Копії через with
        var approvedChanged = approved with { TotalDays = 7 };
        var pendingChanged = pending with { IsUrgent = true };
        var deniedChanged = denied with { Reason = "Пік робіт у відділі" };

        Console.WriteLine();
        Console.WriteLine("=== Змінені копії (with) ===");
        Console.WriteLine(approvedChanged);
        Console.WriteLine(pendingChanged);
        Console.WriteLine(deniedChanged);

        // Оригінали не змінились
        Console.WriteLine();
        Console.WriteLine("=== Оригінали після зміни ===");
        Console.WriteLine(approved);
        Console.WriteLine(pending);
        Console.WriteLine(denied);

        Console.WriteLine();
        Console.WriteLine($"approved == approvedChanged: {approved == approvedChanged}");
        Console.WriteLine($"ReferenceEquals: {ReferenceEquals(approved, approvedChanged)}");
    }
}