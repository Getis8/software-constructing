using System;

public static class Program
{
    public static void Main()
    {
        Console.Write("Введіть кількість хвилин розігріву: ");
        if (!int.TryParse(Console.ReadLine(), out int minutes) || minutes <= 0 || minutes > 1440)
        {
            Console.WriteLine("Некоректне значення (допустимо від 1 до 1440 хвилин).");
            return;
        }

        DateTime start = DateTime.Now;
        TimeSpan duration = TimeSpan.FromMinutes(minutes);
        DateTime end = start.Add(duration);

        Console.WriteLine();
        Console.WriteLine($"Початок розігріву: {start:HH:mm:ss}");
        Console.WriteLine($"Тривалість:        {duration}");
        Console.WriteLine($"Завершення:        {end:HH:mm:ss}");

        if (end.Date > start.Date)
        {
            Console.WriteLine($"Розігрів завершиться {end:dd.MM.yyyy}, {end.DayOfWeek}");
        }
    }
}