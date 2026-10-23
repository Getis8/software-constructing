using System;

// ЛР №4, Завдання 1. Варіант 13 (HR): LeaveRequest

// Базовий абстрактний record із ПРИВАТНИМ конструктором.
// Нащадки оголошені ВСЕРЕДИНІ базового типу, тому тільки вони мають доступ
// до приватного конструктора. Створити іншого нащадка ззовні неможливо.
public abstract record LeaveRequest
{
    private LeaveRequest() { }

    // Схвалено
    public sealed record Approved(DateOnly StartDate, int TotalDays) : LeaveRequest;

    // На розгляді
    public sealed record Pending(string ReviewerName, bool IsUrgent) : LeaveRequest;

    // Відхилено
    public sealed record Denied(string Reason) : LeaveRequest;
}

public static class Program
{
    public static void Main()
    {
        // 1. Створюємо по одному екземпляру кожного підтипу
        var approved = new LeaveRequest.Approved(new DateOnly(2026, 11, 1), 14);
        var pending = new LeaveRequest.Pending("Іваненко І.І.", false);
        var denied = new LeaveRequest.Denied("Недостатньо днів відпустки");

        Console.WriteLine("=== Оригінали ===");
        Console.WriteLine(approved);
        Console.WriteLine(pending);
        Console.WriteLine(denied);

        // 2. Неруйнівна мутація: оператор with створює НОВУ копію зі зміненими полями
        var approvedChanged = approved with { TotalDays = 7 };
        var pendingChanged = pending with { IsUrgent = true };
        var deniedChanged = denied with { Reason = "Пік робіт у відділі" };

        Console.WriteLine();
        Console.WriteLine("=== Змінені копії (with) ===");
        Console.WriteLine(approvedChanged);
        Console.WriteLine(pendingChanged);
        Console.WriteLine(deniedChanged);

        // 3. Оригінали залишились недоторканими
        Console.WriteLine();
        Console.WriteLine("=== Оригінали після зміни (не змінились) ===");
        Console.WriteLine(approved);
        Console.WriteLine(pending);
        Console.WriteLine(denied);

        // 4. Перевірка: це різні об'єкти
        Console.WriteLine();
        Console.WriteLine($"approved == approvedChanged: {approved == approvedChanged}");
        Console.WriteLine($"ReferenceEquals: {ReferenceEquals(approved, approvedChanged)}");

        // 5. Бонус: зіставлення із шаблонами (switch expression)
        Console.WriteLine();
        Console.WriteLine("=== Pattern matching ===");
        LeaveRequest[] all = { approved, pending, denied, pendingChanged };
        foreach (var request in all)
        {
            Console.WriteLine(Describe(request));
        }
    }

    private static string Describe(LeaveRequest request) => request switch
    {
        LeaveRequest.Approved a => $"Схвалено: початок {a.StartDate}, днів: {a.TotalDays}",
        LeaveRequest.Pending p => $"На розгляді у {p.ReviewerName}, термінова: {(p.IsUrgent ? "так" : "ні")}",
        LeaveRequest.Denied d => $"Відхилено, причина: {d.Reason}",
        _ => throw new InvalidOperationException("Невідомий тип заявки")
    };
}
