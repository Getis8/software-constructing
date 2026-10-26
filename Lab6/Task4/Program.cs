using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lab6Task4
{
    // ===== Маркерний атрибут =====
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; set; } = "1.0";

        public ComponentAttribute(string name)
        {
            Name = name;
        }
    }

    // ===== Компоненти з атрибутом =====
    [Component("SmartLamp", Version = "1.2")]
    public class Lamp
    {
        public int Brightness { get; set; }

        public Lamp() { }

        public void Execute()
        {
            Console.WriteLine($"  Lamp.Execute: лампа світить з яскравістю {Brightness}%");
        }

        public override string ToString() => $"Lamp [Brightness={Brightness}]";
    }

    [Component("SmartThermostat", Version = "2.0")]
    public class Thermostat
    {
        public double TargetTemperature { get; set; }

        public Thermostat() { }

        public void Execute()
        {
            Console.WriteLine($"  Thermostat.Execute: підтримується температура {TargetTemperature} °C");
        }

        public override string ToString() => $"Thermostat [TargetTemperature={TargetTemperature}]";
    }

    [Component("SecurityCamera")]
    public class Camera
    {
        public bool IsRecording { get; set; }

        public Camera() { }

        public void Execute()
        {
            Console.WriteLine($"  Camera.Execute: запис {(IsRecording ? "увімкнено" : "вимкнено")}");
        }

        public override string ToString() => $"Camera [IsRecording={IsRecording}]";
    }

    [Component("SmartSpeaker", Version = "3.1")]
    public class Speaker
    {
        public int Volume { get; set; }

        public Speaker() { }

        public void Execute()
        {
            Console.WriteLine($"  Speaker.Execute: гучність виставлено на {Volume}");
        }

        public override string ToString() => $"Speaker [Volume={Volume}]";
    }

    [Component("DoorLock", Version = "1.5")]
    public class DoorLock
    {
        public bool IsLocked { get; set; }

        public DoorLock() { }

        public void Execute()
        {
            Console.WriteLine($"  DoorLock.Execute: двері {(IsLocked ? "зачинено" : "відчинено")}");
        }

        public override string ToString() => $"DoorLock [IsLocked={IsLocked}]";
    }

    [Component("HomeRouter")]
    public class Router
    {
        public string Ssid { get; set; }

        public Router() { }

        public void Execute()
        {
            Console.WriteLine($"  Router.Execute: мережа {Ssid} активна");
        }

        public override string ToString() => $"Router [Ssid={Ssid}]";
    }

    // ===== Класи без атрибута =====
    public class Heater
    {
        public double Power { get; set; }

        public Heater() { }

        public void Execute()
        {
            Console.WriteLine($"  Heater.Execute: обігрів потужністю {Power} Вт");
        }

        public override string ToString() => $"Heater [Power={Power}]";
    }

    public class Fridge
    {
        public double Temperature { get; set; }

        public Fridge() { }

        public void Execute()
        {
            Console.WriteLine($"  Fridge.Execute: охолодження до {Temperature} °C");
        }

        public override string ToString() => $"Fridge [Temperature={Temperature}]";
    }

    public class Curtain
    {
        public int OpenPercent { get; set; }

        public Curtain() { }

        public void Execute()
        {
            Console.WriteLine($"  Curtain.Execute: штори відкриті на {OpenPercent}%");
        }

        public override string ToString() => $"Curtain [OpenPercent={OpenPercent}]";
    }

    public class Sensor
    {
        public string Location { get; set; }

        public Sensor() { }

        public void Execute()
        {
            Console.WriteLine($"  Sensor.Execute: датчик у кімнаті {Location}");
        }

        public override string ToString() => $"Sensor [Location={Location}]";
    }

    // ===== Точка входу =====
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Type[] components = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttribute<ComponentAttribute>() != null)
                .ToArray();

            Console.WriteLine($"Знайдено компонентів: {components.Length}");
            Console.WriteLine();

            foreach (Type type in components)
            {
                ComponentAttribute attr = type.GetCustomAttribute<ComponentAttribute>();
                Console.WriteLine($"=== {type.Name} (name={attr.Name}, version={attr.Version}) ===");

                object instance = Activator.CreateInstance(type);

                PropertyInfo prop = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).First();
                prop.SetValue(instance, GetTestValue(prop.PropertyType));
                Console.WriteLine($"  SetValue: {prop.Name} = {prop.GetValue(instance)}");

                MethodInfo method = type.GetMethod("Execute");
                method.Invoke(instance, null);

                Console.WriteLine($"  ToString: {instance}");
                Console.WriteLine();
            }
        }

        // ----- Значення для встановлення за типом -----
        static object GetTestValue(Type type)
        {
            if (type == typeof(int)) return 75;
            if (type == typeof(double)) return 21.5;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return "HomeNet";
            return null;
        }
    }
}