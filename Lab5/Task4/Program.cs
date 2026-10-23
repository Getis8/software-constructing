using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab5Task4
{
    // Сутність
    public class Device
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public int PowerWatts { get; set; }
        public bool InStock { get; set; }
        public int WarrantyMonths { get; set; }

        public Device(int id, string model, string brand, string category, int powerWatts, bool inStock, int warrantyMonths)
        {
            Id = id;
            Model = model;
            Brand = brand;
            Category = category;
            PowerWatts = powerWatts;
            InStock = inStock;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString()
        {
            return $"#{Id} | {Model} | {Brand} | {Category} | {PowerWatts}W | InStock: {InStock} | Гарантія: {WarrantyMonths} міс.";
        }
    }

    internal class Program
    {
        // Точка входу
        static void Main(string[] args)
        {
            // Колекція
            List<Device> devices = new List<Device>
            {
                new Device(1, "GBH 2-26", "Bosch", "Перфоратори", 800, true, 12),
                new Device(2, "HR2470", "Makita", "Перфоратори", 780, true, 24),
                new Device(3, "DCD777", "DeWalt", "Шуруповерти", 500, true, 36),
                new Device(4, "GSR 12V", "Bosch", "Шуруповерти", 350, true, 30),
                new Device(5, "BS 18", "Metabo", "Шуруповерти", 600, true, 18),
                new Device(6, "9558", "Makita", "Болгарки", 840, true, 12),
                new Device(7, "GWS 750", "Bosch", "Болгарки", 750, true, 36),
                new Device(8, "MS 180", "Stihl", "Пили", 1500, true, 24),
                new Device(9, "PST 650", "Bosch", "Лобзики", 650, false, 48),
                new Device(10, "DW099", "DeWalt", "Лазерні рівні", 5, true, 60),
                new Device(11, "DTW190", "Makita", "Гайковерти", 1000, true, 12),
                new Device(12, "W 2200", "Metabo", "Болгарки", 2200, true, 24)
            };

            // Запит 1: Фільтрація
            List<Device> filtered = devices
                .Where(d => d.InStock && d.PowerWatts <= 1000)
                .ToList();

            Console.WriteLine("1. Фільтрація (в наявності, потужність <= 1000 Вт):");
            foreach (Device d in filtered)
            {
                Console.WriteLine("  " + d);
            }
            Console.WriteLine();

            // Запит 2: Сортування та проєкція
            List<string> models = filtered
                .OrderBy(d => d.Category)
                .ThenByDescending(d => d.PowerWatts)
                .Select(d => d.Model)
                .ToList();

            Console.WriteLine("2. Сортування (категорія ↑, потужність ↓), назви моделей:");
            foreach (string m in models)
            {
                Console.WriteLine("  " + m);
            }
            Console.WriteLine();

            // Запит 3: Пагінація
            List<string> page = filtered
                .OrderBy(d => d.Category)
                .ThenByDescending(d => d.PowerWatts)
                .Select(d => d.Model)
                .Skip(1)
                .Take(7)
                .ToList();

            Console.WriteLine("3. Пагінація (Skip 1, Take 7):");
            foreach (string m in page)
            {
                Console.WriteLine("  " + m);
            }
            Console.WriteLine();

            // Запит 4: Пошук одного елемента
            Device? first = devices.FirstOrDefault(d => d.Brand == "Bosch" && d.WarrantyMonths > 24);

            Console.WriteLine("4. Перший пристрій Bosch з гарантією понад 24 міс.:");
            Console.WriteLine(first != null ? "  " + first : "  (такого пристрою немає)");
            Console.WriteLine();

            // Запит 5: Екстремум
            Device? longestWarranty = devices.MaxBy(d => d.WarrantyMonths);

            Console.WriteLine("5. Найдовший гарантійний термін:");
            Console.WriteLine(longestWarranty != null ? "  " + longestWarranty : "  (колекція порожня)");
        }
    }
}