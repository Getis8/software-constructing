using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab6Task2
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

    // ===== Результат валідації =====
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();

        public void AddError(string memberName, string message)
        {
            Errors.Add($"[{memberName}] {message}");
        }
    }

    // ===== Сервіс валідації =====
    public class ValidatorService
    {
        public ValidationResult Validate(object entity)
        {
            ValidationResult result = new ValidationResult();

            if (entity == null)
            {
                result.AddError("entity", "Об'єкт не може бути null");
                return result;
            }

            Type type = entity.GetType();

            ValidateClass(entity, type, result);
            ValidateProperties(entity, type, result);

            return result;
        }

        // ----- Валідація рівня класу -----
        private void ValidateClass(object entity, Type type, ValidationResult result)
        {
            HighPowerZoneAttribute attr = type.GetCustomAttribute<HighPowerZoneAttribute>();
            if (attr == null)
            {
                return;
            }

            PropertyInfo powerProp = type.GetProperty(attr.PowerPropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PropertyInfo zoneProp = type.GetProperty(attr.ZonePropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (powerProp == null || zoneProp == null)
            {
                result.AddError(type.Name, "Не знайдено властивості, зазначені в атрибуті класу");
                return;
            }

            double power = Convert.ToDouble(powerProp.GetValue(entity));
            bool inZone = Convert.ToBoolean(zoneProp.GetValue(entity));

            if (power > attr.PowerThreshold && !inZone)
            {
                result.AddError(
                    $"{powerProp.Name}, {zoneProp.Name}",
                    $"Пристрої потужністю понад {attr.PowerThreshold} Вт можна зберігати лише у спеціалізованій зоні складу");
            }
        }

        // ----- Валідація властивостей -----
        private void ValidateProperties(object entity, Type type, ValidationResult result)
        {
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (PropertyInfo prop in properties)
            {
                object value = prop.GetValue(entity);

                RegexMatchAttribute regexAttr = prop.GetCustomAttribute<RegexMatchAttribute>();
                if (regexAttr != null)
                {
                    string text = value as string;
                    if (text == null || !Regex.IsMatch(text, regexAttr.Pattern))
                    {
                        result.AddError(prop.Name, regexAttr.ErrorMessage ?? $"Значення не відповідає шаблону {regexAttr.Pattern}");
                    }
                }

                ValueRangeAttribute rangeAttr = prop.GetCustomAttribute<ValueRangeAttribute>();
                if (rangeAttr != null)
                {
                    double number = Convert.ToDouble(value);
                    if (number < rangeAttr.Min || number > rangeAttr.Max)
                    {
                        result.AddError(prop.Name, rangeAttr.ErrorMessage ?? $"Значення має бути в межах {rangeAttr.Min}-{rangeAttr.Max}");
                    }
                }
            }
        }
    }

    // ===== Точка входу =====
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Device validDevice = new Device
            {
                SerialNumber = "ABC-123456",
                PowerWatts = 1500,
                Quantity = 3,
                IsInSpecializedZone = true,
                ManufactureDate = new DateTime(2024, 5, 20)
            };

            Device invalidDevice = new Device
            {
                SerialNumber = "12-XYZ",
                PowerWatts = 6000,
                Quantity = 1,
                IsInSpecializedZone = false,
                ManufactureDate = new DateTime(2024, 5, 20)
            };

            ValidatorService validator = new ValidatorService();

            PrintResult("Валідний пристрій", validDevice, validator.Validate(validDevice));
            PrintResult("Невалідний пристрій", invalidDevice, validator.Validate(invalidDevice));
        }

        static void PrintResult(string title, Device device, ValidationResult result)
        {
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine(device);
            Console.WriteLine($"IsValid: {result.IsValid}");

            foreach (string error in result.Errors)
            {
                Console.WriteLine($" - {error}");
            }

            Console.WriteLine();
        }
    }
}