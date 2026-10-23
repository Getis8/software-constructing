using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab5Task5
{
    // Сутність: пристрій
    public class Device
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public int PowerWatts { get; set; }
        public bool InStock { get; set; }
        public int WarrantyMonths { get; set; }
        public int WarehouseId { get; set; }

        public Device(int id, string model, string brand, string category, int powerWatts, bool inStock, int warrantyMonths, int warehouseId)
        {
            Id = id;
            Model = model;
            Brand = brand;
            Category = category;
            PowerWatts = powerWatts;
            InStock = inStock;
            WarrantyMonths = warrantyMonths;
            WarehouseId = warehouseId;
        }
    }

    // Сутність: склад
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }

        public Warehouse(int id, string name, string city)
        {
            Id = id;
            Name = name;
            City = city;
        }
    }

    internal class Program
    {
        // Точка входу
        static void Main(string[] args)
        {
            // Колекція складів
            List<Warehouse> warehouses = new List<Warehouse>
            {
                new Warehouse(1, "Центральний склад", "Чернівці"),
                new Warehouse(2, "Північний склад", "Київ"),
                new Warehouse(3, "Західний склад", "Львів"),
                new Warehouse(4, "Резервний склад", "Одеса")
            };

            // Колекція пристроїв
            List<Device> devices = new List<Device>
            {
                new Device(1, "GBH 2-26", "Bosch", "Перфоратори", 800, true, 12, 1),
                new Device(2, "HR2470", "Makita", "Перфоратори", 780, true, 24, 1),
                new Device(3, "DCD777", "DeWalt", "Шуруповерти", 500, true, 36, 2),
                new Device(4, "GSR 12V", "Bosch", "Шуруповерти", 350, true, 30, 2),
                new Device(5, "BS 18", "Metabo", "Шуруповерти", 600, true, 18, 1),
                new Device(6, "9558", "Makita", "Болгарки", 840, true, 12, 3),
                new Device(7, "GWS 750", "Bosch", "Болгарки", 750, true, 36, 3),
                new Device(8, "MS 180", "Stihl", "Пили", 1500, true, 24, 2),
                new Device(9, "PST 650", "Bosch", "Лобзики", 650, false, 48, 3),
                new Device(10, "DW099", "DeWalt", "Лазерні рівні", 5, true, 60, 1),
                new Device(11, "DTW190", "Makita", "Гайковерти", 1000, true, 12, 2),
                new Device(12, "W 2200", "Metabo", "Болгарки", 2200, true, 24, 3)
            };

            // Запит 1: Групування за складом
            var groups = devices
                .GroupBy(d => d.WarehouseId)
                .Select(g => new { WarehouseId = g.Key, Count = g.Count() })
                .OrderBy(x => x.WarehouseId);

            Console.WriteLine("1. Кількість пристроїв на складах:");
            foreach (var g in groups)
            {
                Console.WriteLine($"  Склад {g.WarehouseId}: {g.Count}");
            }
            Console.WriteLine();

            // Запит 2: Середня потужність по складах
            var avgPower = devices
                .GroupBy(d => d.WarehouseId)
                .Select(g => new { WarehouseId = g.Key, Avg = g.Average(d => d.PowerWatts) })
                .OrderBy(x => x.WarehouseId);

            Console.WriteLine("2. Середня потужність по складах:");
            foreach (var a in avgPower)
            {
                Console.WriteLine($"  Склад {a.WarehouseId}: {a.Avg:F2} Вт");
            }
            Console.WriteLine();

            // Запит 3: Inner Join
            var joined = devices
                .Join(warehouses,
                      d => d.WarehouseId,
                      w => w.Id,
                      (d, w) => new { d.Model, d.PowerWatts, WarehouseName = w.Name });

            Console.WriteLine("3. Join (модель, потужність, склад):");
            foreach (var j in joined)
            {
                Console.WriteLine($"  {j.Model} | {j.PowerWatts} Вт | {j.WarehouseName}");
            }
            Console.WriteLine();

            // Запит 4: GroupJoin
            var groupJoined = warehouses
                .GroupJoin(devices,
                           w => w.Id,
                           d => d.WarehouseId,
                           (w, ds) => new { w.Name, Devices = ds.ToList() });

            Console.WriteLine("4. GroupJoin (склад і його пристрої):");
            foreach (var gj in groupJoined)
            {
                Console.WriteLine($"  {gj.Name}:");
                if (gj.Devices.Count == 0)
                {
                    Console.WriteLine("    (пристроїв немає)");
                }
                foreach (Device d in gj.Devices)
                {
                    Console.WriteLine($"    {d.Model}");
                }
            }
            Console.WriteLine();

            // Запит 5: Aggregate
            string line = devices
                .Where(d => d.InStock)
                .Select(d => d.Model)
                .Aggregate((acc, next) => acc + "\t" + next);

            Console.WriteLine("5. Пристрої в наявності (через табуляцію):");
            Console.WriteLine(line);
        }
    }
}