using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lab5Task2
{
    internal class Program
    {
        // Зчитування чисел з файлу
        static List<int> ReadList(string path)
        {
            List<int> list = new List<int>();
            string text = File.ReadAllText(path);
            string[] parts = text.Split(new char[] { ' ', ',', ';', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string p in parts)
            {
                list.Add(int.Parse(p));
            }
            return list;
        }

        // Запис у файл по вісім елементів у рядку
        static void WriteList(string path, List<int> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i]);
                if ((i + 1) % 8 == 0 || i == list.Count - 1)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(' ');
                }
            }
            File.WriteAllText(path, sb.ToString());
        }

        // Точка входу
        static void Main(string[] args)
        {
            string inputPath = "input.txt";
            string outputPath = "output.txt";

            if (!File.Exists(inputPath))
            {
                File.WriteAllText(inputPath, "5 12 -3 8 20 7 1 15 9 30 4 10 -8 6 25 11 2 18");
                Console.WriteLine("Створено input.txt з тестовими даними.");
            }

            List<int> L = ReadList(inputPath);

            Console.Write("Введіть a: ");
            int a = int.Parse(Console.ReadLine());
            Console.Write("Введіть b (b > a): ");
            int b = int.Parse(Console.ReadLine());

            if (a >= b)
            {
                Console.WriteLine("Помилка: потрібно a < b.");
                return;
            }

            // Один прохід: розкладання по трьох групах
            List<int> less = new List<int>();
            List<int> between = new List<int>();
            List<int> greater = new List<int>();

            foreach (int x in L)
            {
                if (x < a)
                {
                    less.Add(x);
                }
                else if (x > b)
                {
                    greater.Add(x);
                }
                else if (x > a && x < b)
                {
                    between.Add(x);
                }
            }

            // Модифікований список
            List<int> result = new List<int>();
            result.AddRange(less);
            result.AddRange(between);
            result.AddRange(greater);

            Console.WriteLine("Менші за a: " + string.Join(" ", less));
            Console.WriteLine("В діапазоні (a, b): " + string.Join(" ", between));
            Console.WriteLine("Більші за b: " + string.Join(" ", greater));

            WriteList(outputPath, result);
            Console.WriteLine("Результат збережено у " + outputPath);
        }
    }
}