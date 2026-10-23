using System;

[Flags]
public enum PlayerRoles
{
    None = 0,
    Captain = 1,
    Goalkeeper = 2,
    Striker = 4,
    Defender = 8
}

// Мутабельна структура
public struct ActivePlayerStats
{
    public string Name;
    public int Goals;
    public int MinutesPlayed;
    public PlayerRoles Roles;

    public ActivePlayerStats(string name, int goals, int minutesPlayed, PlayerRoles roles)
    {
        Name = name;
        Goals = goals;
        MinutesPlayed = minutesPlayed;
        Roles = roles;
    }

    public override string ToString() =>
        $"{Name}: голів {Goals}, хвилин {MinutesPlayed}, ролі [{Roles}]";
}

// Незмінна структура
public readonly struct HallOfFameRecord
{
    public string PlayerName { get; }
    public int TotalGoals { get; }
    public int InductionYear { get; }
    public PlayerRoles MainRole { get; }

    public HallOfFameRecord(string playerName, int totalGoals, int inductionYear, PlayerRoles mainRole)
    {
        PlayerName = playerName;
        TotalGoals = totalGoals;
        InductionYear = inductionYear;
        MainRole = mainRole;
    }

    public override string ToString() =>
        $"{PlayerName}: голів {TotalGoals}, рік вступу {InductionYear}, роль [{MainRole}]";
}

public static class Program
{
    public static void Main()
    {
        // Enum [Flags]
        Console.WriteLine("=== Enum [Flags] ===");
        var roles = PlayerRoles.Captain | PlayerRoles.Striker;
        Console.WriteLine($"Ролі: {roles}");
        Console.WriteLine($"Є капітаном: {roles.HasFlag(PlayerRoles.Captain)}");
        Console.WriteLine($"Є воротарем: {roles.HasFlag(PlayerRoles.Goalkeeper)}");

        // Copy-by-value
        Console.WriteLine();
        Console.WriteLine("=== Передача за значенням ===");
        var stats = new ActivePlayerStats("Шевченко", 0, 90, roles);
        Console.WriteLine($"До виклику:    {stats}");
        ScoreGoal(stats);
        Console.WriteLine($"Після виклику: {stats}");

        // Copy-by-reference (ref)
        Console.WriteLine();
        Console.WriteLine("=== Передача через ref ===");
        Console.WriteLine($"До виклику:    {stats}");
        ScoreGoalRef(ref stats);
        Console.WriteLine($"Після виклику: {stats}");

        // Readonly struct
        Console.WriteLine();
        Console.WriteLine("=== Readonly struct ===");
        var record = new HallOfFameRecord("Шевченко", 175, 2018, PlayerRoles.Striker);
        Console.WriteLine(record);
        // record.TotalGoals = 200; // помилка компіляції: властивість лише для читання
    }

    // Копія структури, оригінал не змінюється
    private static void ScoreGoal(ActivePlayerStats stats)
    {
        stats.Goals++;
        Console.WriteLine($"  Всередині ScoreGoal: {stats}");
    }

    // Працює з оригіналом
    private static void ScoreGoalRef(ref ActivePlayerStats stats)
    {
        stats.Goals++;
        Console.WriteLine($"  Всередині ScoreGoalRef: {stats}");
    }
}