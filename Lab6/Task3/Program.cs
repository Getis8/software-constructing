using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab6Task3
// dotnet run -c Release
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

    // ===== Спільний інтерфейс =====
    public interface IValidationService
    {
        ValidationResult Validate(object entity);
    }

    // ===== Базовий сервіс валідації (без кешу) =====
    public class ValidatorService : IValidationService
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

    // ===== Метадані одного поля =====
    public class PropertyValidationMetadata
    {
        public PropertyInfo Property { get; set; }
        public RegexMatchAttribute RegexAttr { get; set; }
        public Regex CompiledRegex { get; set; }
        public ValueRangeAttribute RangeAttr { get; set; }
    }

    // ===== Метадані класу =====
    public class ClassValidationMetadata
    {
        public HighPowerZoneAttribute ClassAttr { get; set; }
        public PropertyInfo PowerProp { get; set; }
        public PropertyInfo ZoneProp { get; set; }
        public List<PropertyValidationMetadata> Properties { get; } = new List<PropertyValidationMetadata>();
    }

    // ===== Сервіс валідації з кешем =====
    public class CachedValidationService : IValidationService
    {
        private readonly Dictionary<Type, ClassValidationMetadata> _cache = new Dictionary<Type, ClassValidationMetadata>();

        public ValidationResult Validate(object entity)
        {
            ValidationResult result = new ValidationResult();

            if (entity == null)
            {
                result.AddError("entity", "Об'єкт не може бути null");
                return result;
            }

            Type type = entity.GetType();

            if (!_cache.TryGetValue(type, out ClassValidationMetadata meta))
            {
                meta = BuildMetadata(type);
                _cache[type] = meta;
            }

            ValidateClass(entity, type, meta, result);
            ValidateProperties(entity, meta, result);

            return result;
        }

        // ----- Побудова метаданих -----
        private ClassValidationMetadata BuildMetadata(Type type)
        {
            ClassValidationMetadata meta = new ClassValidationMetadata();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            meta.ClassAttr = type.GetCustomAttribute<HighPowerZoneAttribute>();
            if (meta.ClassAttr != null)
            {
                meta.PowerProp = type.GetProperty(meta.ClassAttr.PowerPropertyName, flags);
                meta.ZoneProp = type.GetProperty(meta.ClassAttr.ZonePropertyName, flags);
            }

            foreach (PropertyInfo prop in type.GetProperties(flags))
            {
                RegexMatchAttribute regexAttr = prop.GetCustomAttribute<RegexMatchAttribute>();
                ValueRangeAttribute rangeAttr = prop.GetCustomAttribute<ValueRangeAttribute>();

                if (regexAttr == null && rangeAttr == null)
                {
                    continue;
                }

                meta.Properties.Add(new PropertyValidationMetadata
                {
                    Property = prop,
                    RegexAttr = regexAttr,
                    CompiledRegex = regexAttr != null ? new Regex(regexAttr.Pattern) : null,
                    RangeAttr = rangeAttr
                });
            }

            return meta;
        }

        // ----- Валідація рівня класу -----
        private void ValidateClass(object entity, Type type, ClassValidationMetadata meta, ValidationResult result)
        {
            if (meta.ClassAttr == null)
            {
                return;
            }

            if (meta.PowerProp == null || meta.ZoneProp == null)
            {
                result.AddError(type.Name, "Не знайдено властивості, зазначені в атрибуті класу");
                return;
            }

            double power = Convert.ToDouble(meta.PowerProp.GetValue(entity));
            bool inZone = Convert.ToBoolean(meta.ZoneProp.GetValue(entity));

            if (power > meta.ClassAttr.PowerThreshold && !inZone)
            {
                result.AddError(
                    $"{meta.PowerProp.Name}, {meta.ZoneProp.Name}",
                    $"Пристрої потужністю понад {meta.ClassAttr.PowerThreshold} Вт можна зберігати лише у спеціалізованій зоні складу");
            }
        }

        // ----- Валідація властивостей -----
        private void ValidateProperties(object entity, ClassValidationMetadata meta, ValidationResult result)
        {
            foreach (PropertyValidationMetadata pm in meta.Properties)
            {
                object value = pm.Property.GetValue(entity);

                if (pm.RegexAttr != null)
                {
                    string text = value as string;
                    if (text == null || !pm.CompiledRegex.IsMatch(text))
                    {
                        result.AddError(pm.Property.Name, pm.RegexAttr.ErrorMessage ?? $"Значення не відповідає шаблону {pm.RegexAttr.Pattern}");
                    }
                }

                if (pm.RangeAttr != null)
                {
                    double number = Convert.ToDouble(value);
                    if (number < pm.RangeAttr.Min || number > pm.RangeAttr.Max)
                    {
                        result.AddError(pm.Property.Name, pm.RangeAttr.ErrorMessage ?? $"Значення має бути в межах {pm.RangeAttr.Min}-{pm.RangeAttr.Max}");
                    }
                }
            }
        }
    }

    // ===== Точка входу =====
    internal class Program
    {
        const int Iterations = 100000;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Device[] devices = GenerateDevices(Iterations);

            IValidationService basic = new ValidatorService();
            IValidationService cached = new CachedValidationService();

            // Прогрів
            for (int i = 0; i < 1000; i++)
            {
                basic.Validate(devices[i]);
                cached.Validate(devices[i]);
            }

            long basicTime = Measure(basic, devices, out int basicValid);
            long cachedTime = Measure(cached, devices, out int cachedValid);

            Console.WriteLine($"Ітерацій: {Iterations}");
            Console.WriteLine();
            Console.WriteLine("+--------------------------+-----------+---------------+");
            Console.WriteLine("| Сервіс                   | Час, мс   | Валідних      |");
            Console.WriteLine("+--------------------------+-----------+---------------+");
            Console.WriteLine($"| {"ValidatorService",-24} | {basicTime,9} | {basicValid,13} |");
            Console.WriteLine($"| {"CachedValidationService",-24} | {cachedTime,9} | {cachedValid,13} |");
            Console.WriteLine("+--------------------------+-----------+---------------+");
            Console.WriteLine();

            if (cachedTime > 0)
            {
                Console.WriteLine($"Прискорення: у {(double)basicTime / cachedTime:F2} раза");
            }
        }

        // ----- Замір часу -----
        static long Measure(IValidationService service, Device[] devices, out int validCount)
        {
            validCount = 0;
            Stopwatch sw = Stopwatch.StartNew();

            foreach (Device d in devices)
            {
                if (service.Validate(d).IsValid)
                {
                    validCount++;
                }
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        // ----- Генерація даних -----
        static Device[] GenerateDevices(int count)
        {
            Random rnd = new Random();
            Device[] result = new Device[count];

            for (int i = 0; i < count; i++)
            {
                string serial = rnd.Next(0, 2) == 0
                    ? $"{RandomLetters(rnd, 3)}-{rnd.Next(100000, 999999)}"
                    : $"{rnd.Next(10, 99)}-{RandomLetters(rnd, 3)}";

                result[i] = new Device
                {
                    SerialNumber = serial,
                    PowerWatts = rnd.Next(0, 6000),
                    Quantity = rnd.Next(1, 100),
                    IsInSpecializedZone = rnd.Next(0, 2) == 0,
                    ManufactureDate = DateTime.Now.AddDays(-rnd.Next(0, 3000))
                };
            }

            return result;
        }

        static string RandomLetters(Random rnd, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)rnd.Next('A', 'Z' + 1);
            }
            return new string(chars);
        }
    }
}