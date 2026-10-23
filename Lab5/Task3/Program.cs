using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Lab5Task3
{
    internal class Program
    {
        // Зчитування словника з файлу (формат рядка: ключ значення)
        static Dictionary<string, double> ReadDictionary(string path)
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();
            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                int idx = line.LastIndexOf(' ');
                string key = line.Substring(0, idx).Trim();
                double value = double.Parse(line.Substring(idx + 1).Replace(',', '.'), CultureInfo.InvariantCulture);
                dict[key] = value;
            }
            return dict;
        }

        // Безпечне додавання або оновлення
        static void AddOrUpdate(Dictionary<string, double> dict, string key, double value)
        {
            if (dict.ContainsKey(key))
            {
                Console.WriteLine($"Оновлення: {key} {dict[key]} -> {value}");
                dict[key] = value;
            }
            else
            {
                Console.WriteLine($"Додавання: {key} = {value}");
                dict.Add(key, value);
            }
        }

        // Безпечне вилучення
        static void RemoveKey(Dictionary<string, double> dict, string key)
        {
            if (dict.Remove(key))
            {
                Console.WriteLine($"Вилучено: {key}");
            }
            else
            {
                Console.WriteLine($"Ключ {key} не знайдено");
            }
        }

        // Безпечний доступ за ключем
        static void PrintValue(Dictionary<string, double> dict, string key)
        {
            if (dict.TryGetValue(key, out double value))
            {
                Console.WriteLine($"{key} = {value}");
            }
            else
            {
                Console.WriteLine($"{key}: ключа немає");
            }
        }

        // Видалення аномальних значень
        static void RemoveOutliers(Dictionary<string, double> dict, double k)
        {
            if (dict.Count == 0)
            {
                return;
            }

            double avg = dict.Values.Average();
            double limit = Math.Abs(avg) * k / 100.0;
            Console.WriteLine($"Середнє: {avg:F2}, допустиме відхилення: {limit:F2}");

            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, double> pair in dict)
            {
                if (Math.Abs(pair.Value - avg) > limit)
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (string key in toRemove)
            {
                dict.Remove(key);
                Console.WriteLine($"Аномалія вилучена: {key}");
            }
        }

        // Виведення словника, відсортованого за значенням
        static void PrintSorted(string title, Dictionary<string, double> dict)
        {
            Console.WriteLine(title);
            foreach (KeyValuePair<string, double> pair in dict.OrderBy(p => p.Value))
            {
                Console.WriteLine($"  {pair.Key}: {pair.Value:F2}");
            }
            Console.WriteLine();
        }

        // Точка входу
        static void Main(string[] args)
        {
            string inputPath = "input.txt";

            if (!File.Exists(inputPath))
            {
                File.WriteAllText(inputPath,
                    "Яблука 10\nБанани 12\nГруші 11\nСливи 13\nВишні 9\nАвокадо 50\n");
                Console.WriteLine("Створено input.txt з тестовими даними.");
            }

            Dictionary<string, double> dict = ReadDictionary(inputPath);
            PrintSorted("Початковий словник:", dict);

            // Базові операції
            PrintValue(dict, "Груші");
            PrintValue(dict, "Манго");
            AddOrUpdate(dict, "Манго", 14);
            AddOrUpdate(dict, "Банани", 12.5);
            RemoveKey(dict, "Сливи");
            RemoveKey(dict, "Ківі");
            Console.WriteLine();

            PrintSorted("Після додавання/оновлення/вилучення:", dict);

            Console.Write("Введіть K (відсотки): ");
            double k = double.Parse(Console.ReadLine().Replace(',', '.'), CultureInfo.InvariantCulture);

            RemoveOutliers(dict, k);
            Console.WriteLine();

            PrintSorted("Результат (за зростанням значень):", dict);
        }
    }
}