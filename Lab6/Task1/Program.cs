using System;

namespace Lab6Task1
{
    // ===== Атрибут рівня класу =====
    [AttributeUsage(AttributeTargets.Class)]
    public class HighPowerZoneAttribute : Attribute
    {
        public string PowerPropertyName { get; }
        public string ZonePropertyName { get; }
        public double PowerThreshold { get; }

        public HighPowerZoneAttribute(string powerPropertyName, string zonePropertyName, double powerThreshold)
        {
            PowerPropertyName = powerPropertyName;
            ZonePropertyName = zonePropertyName;
            PowerThreshold = powerThreshold;
        }
    }

    // ===== Атрибут поля: регулярний вираз =====
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RegexMatchAttribute : Attribute
    {
        public string Pattern { get; }
        public string ErrorMessage { get; set; }

        public RegexMatchAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }

    // ===== Атрибут поля: діапазон значень =====
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ValueRangeAttribute : Attribute
    {
        public double Min { get; }
        public double Max { get; }
        public string ErrorMessage { get; set; }

        public ValueRangeAttribute(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }

    // ===== Доменна сутність =====
    [HighPowerZone(nameof(PowerWatts), nameof(IsInSpecializedZone), 1000)]
    public class Device
    {
        [RegexMatch(@"^[A-Z]{3}-\d{6}$", ErrorMessage = "Серійний номер має формат AAA-123456")]
        public string SerialNumber { get; set; }

        [ValueRange(10, 5000, ErrorMessage = "Потужність має бути в межах 10-5000 Вт")]
        public double PowerWatts { get; set; }

        public int Quantity { get; set; }

        public bool IsInSpecializedZone { get; set; }

        public DateTime ManufactureDate { get; set; }

        public override string ToString()
        {
            return $"Device: {SerialNumber}, {PowerWatts} W, qty={Quantity}, specialZone={IsInSpecializedZone}, date={ManufactureDate:dd.MM.yyyy}";
        }
    }

    // ===== Точка входу =====
    internal class Program
    {
        static void Main(string[] args)
        {
            Device device = new Device
            {
                SerialNumber = "ABC-123456",
                PowerWatts = 1500,
                Quantity = 3,
                IsInSpecializedZone = true,
                ManufactureDate = new DateTime(2024, 5, 20)
            };

            Console.WriteLine(device);
        }
    }
}