using System;
using System.Collections.Generic;

namespace Lab5Task1
{
    // Сутність
    public class Device
    {
        public string SerialNumber { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public int PowerWatts { get; set; }
        public bool InStock { get; set; }
        public string WarehouseZone { get; set; }
        public int WarrantyMonthsLeft { get; set; }

        public Device(string serialNumber, string name, string brand, int powerWatts, bool inStock, string warehouseZone, int warrantyMonthsLeft)
        {
            SerialNumber = serialNumber;
            Name = name;
            Brand = brand;
            PowerWatts = powerWatts;
            InStock = inStock;
            WarehouseZone = warehouseZone;
            WarrantyMonthsLeft = warrantyMonthsLeft;
        }

        public override string ToString()
        {
            return $"{SerialNumber} | {Name} | {Brand} | {PowerWatts}W | InStock: {InStock} | {WarehouseZone} | Warranty: {WarrantyMonthsLeft}m";
        }
    }

    // Узагальнене сховище
    public class StorageManager<T>
    {
        private readonly List<T> _items = new List<T>();

        // Додавання
        public void Add(T item)
        {
            _items.Add(item);
        }

        // Видалення
        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        // Пошук за критерієм
        public T? Find(Func<T, bool> criteria)
        {
            foreach (T item in _items)
            {
                if (criteria(item))
                {
                    return item;
                }
            }
            return default;
        }

        // Фільтрація через цикл
        public IReadOnlyList<T> Filter(Func<T, bool> predicate)
        {
            List<T> result = new List<T>();
            foreach (T item in _items)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
            return result.AsReadOnly();
        }

        public int Count
        {
            get { return _items.Count; }
        }
    }

    // Точка входу
    internal class Program
    {
        static void PrintList(string title, IReadOnlyList<Device> list)
        {
            Console.WriteLine(title);
            if (list.Count == 0)
            {
                Console.WriteLine("  (нічого не знайдено)");
            }
            foreach (Device d in list)
            {
                Console.WriteLine("  " + d);
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            StorageManager<Device> storage = new StorageManager<Device>();

            storage.Add(new Device("SN-11223344", "Перфоратор", "Bosch", 800, true, "Zone-A", 12));
            storage.Add(new Device("SN-99887766", "Болгарка", "Makita", 2200, true, "Zone-B", 3));
            storage.Add(new Device("SN-55667788", "Шуруповерт", "DeWalt", 500, false, "Zone-B", 8));
            storage.Add(new Device("SN-22334455", "Компресор", "Metabo", 1800, true, "Zone-B", 2));
            storage.Add(new Device("SN-66778899", "Лобзик", "Bosch", 650, false, "Zone-C", 0));
            storage.Add(new Device("SN-33445566", "Пилка", "Stihl", 1500, true, "Zone-A", 6));

            // Пошук за серійним номером
            Device? found = storage.Find(d => d.SerialNumber == "SN-99887766");
            Console.WriteLine("Пошук за серійним номером:");
            Console.WriteLine(found != null ? "  " + found : "  (не знайдено)");
            Console.WriteLine();

            // Предикат 1
            PrintList("1. PowerWatts <= 1500:",
                storage.Filter(d => d.PowerWatts <= 1500));

            // Предикат 2
            PrintList("2. InStock == true AND WarehouseZone == Zone-B:",
                storage.Filter(d => d.InStock && d.WarehouseZone == "Zone-B"));

            // Предикат 3
            PrintList("3. WarrantyMonthsLeft >= 6 OR Brand == Bosch:",
                storage.Filter(d => d.WarrantyMonthsLeft >= 6 || d.Brand == "Bosch"));

            // Видалення
            if (found != null)
            {
                bool removed = storage.Remove(found);
                Console.WriteLine("Видалення SN-99887766: " + (removed ? "успішно" : "не вдалося"));
                Console.WriteLine("Кількість елементів: " + storage.Count);
            }
        }
    }
}