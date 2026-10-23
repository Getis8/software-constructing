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
        Console.WriteLine("=== Маршрутизація заявок ===");
        Console.WriteLine();

        LeaveRequest[] requests =
        {
            new LeaveRequest.Approved(new DateOnly(2026, 12, 20), 21),
            new LeaveRequest.Approved(new DateOnly(2026, 11, 5), 2),
            new LeaveRequest.Approved(new DateOnly(2026, 11, 10), 7),

            new LeaveRequest.Pending("Петренко П.П.", true),
            new LeaveRequest.Pending("", false),
            new LeaveRequest.Pending("Коваль О.В.", false),

            new LeaveRequest.Denied("Недостатньо днів відпустки"),
            new LeaveRequest.Denied("Пік робіт у відділі"),
            new LeaveRequest.Denied(""),
        };

        foreach (var request in requests)
        {
            Console.WriteLine($"{request}\n   -> {Route(request)}\n");
        }
    }

    private static string Route(LeaveRequest request) => request switch
    {
        // Approved
        LeaveRequest.Approved { TotalDays: > 14 }
            => "Передати в бухгалтерію: розрахунок відпускних за довгу відпустку",
        LeaveRequest.Approved(_, <= 3)
            => "Оформити наказ без додаткових погоджень (коротка відпустка)",
        LeaveRequest.Approved(var start, var days)
            => $"Оформити стандартний наказ: початок {start}, тривалість {days} дн.",

        // Pending
        LeaveRequest.Pending { IsUrgent: true }
            => "Ескалювати на керівника департаменту",
        LeaveRequest.Pending(var reviewer, false) when string.IsNullOrWhiteSpace(reviewer)
            => "Призначити рецензента для розгляду заявки",
        LeaveRequest.Pending(var reviewer, false)
            => $"Очікувати рішення від {reviewer}",

        // Denied
        LeaveRequest.Denied { Reason: null or "" }
            => "Запросити в рецензента причину відмови",
        LeaveRequest.Denied(var reason) when reason.Contains("відпустк", StringComparison.OrdinalIgnoreCase)
            => "Повідомити працівника та запропонувати інші дати",
        LeaveRequest.Denied(var reason)
            => $"Повідомити працівника про відмову. Причина: {reason}",

        null => throw new ArgumentNullException(nameof(request))
    };
}