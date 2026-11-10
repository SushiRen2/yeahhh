using System;
using System.Collections.Generic;
using System.IO;

// ABSTRACTION: children fill in the details
abstract class Machine
{
    public string Name { get; set; }
    public int ManufactureYear { get; set; }
    public DateTime LastChecked { get; set; }s

    public abstract int GetLubricationMonths();
    public abstract int GetLifespanYears();
    public abstract string GetTypeName();

    // ENCAPSULATION: private data, protected by a guard
    private double temperature;

    public double Temperature
    {
        get { return temperature; }
        set
        {
            if (value >= -50 && value <= 60)
            {
                temperature = value;
            }
            else
            {
                Console.WriteLine("Invalid temperature for " + Name);
            }
        }
    }

    public int GetMonthsSinceChecked()
    {
        DateTime today = DateTime.Now;
        return (today.Year - LastChecked.Year) * 12 + (today.Month - LastChecked.Month);
    }

    public string GetReminder()
    {
        int interval = GetLubricationMonths();

        if (Temperature > 30)
        {
            interval = interval / 2;
        }

        int age = DateTime.Now.Year - ManufactureYear;
        int months = GetMonthsSinceChecked();
        string info = Name + " (last checked " + LastChecked.ToString("yyyy-MM-dd") + "): ";

        if (age >= GetLifespanYears())
        {
            return info + "REPLACE the machine";
        }
        else if (months >= interval)
        {
            return info + "LUBRICATE NOW";
        }
        else
        {
            return info + "OK, next lubrication in " + (interval - months) + " month(s)";
        }
    }

    public virtual string ToFileLine()
    {
        return GetTypeName() + "|" + Name + "|" + ManufactureYear + "|" +
               LastChecked.ToString("yyyy-MM-dd") + "|" + Temperature;
    }
}

// INHERITANCE: children reuse the parent's code
class CncMachine : Machine
{
    public override int GetLubricationMonths() { return 4; }

    public override int GetLifespanYears() { return 15; }

    public override string GetTypeName() { return "CNC"; }
}

// Inherits Machine + adds IsOutdoor
class Generator : Machine
{
    public bool IsOutdoor { get; set; }

    public override int GetLubricationMonths()
    {
        if (IsOutdoor)
        {
            return 2;
        }
        return 4;
    }

    public override int GetLifespanYears() { return 10; }

    public override string GetTypeName() { return "Generator"; }

    // override = change parent's version, base = reuse it
    public override string ToFileLine()
    {
        return base.ToFileLine() + "|" + IsOutdoor;
    }
}

class AirCompressor : Machine
{
    public override int GetLubricationMonths() { return 3; }

    public override int GetLifespanYears() { return 12; }

    public override string GetTypeName() { return "Compressor"; }
}

class Program
{
    static string fileName = "machines.txt";

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        List<Machine> machines = LoadMachines();
        bool running = true;

        while (running)
        {
            Console.Clear();
            ShowMenu(machines);
            string choice = Console.ReadLine();
            Console.Clear();

            if (choice == "1")
            {
                AddMachine(machines);
            }
            else if (choice == "2")
            {
                ShowReminders(machines);
            }
            else if (choice == "3")
            {
                UpdateLastChecked(machines);
            }
            else if (choice == "4")
            {
                running = false;
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }

            if (running)
            {
                Console.WriteLine();
                Console.Write("Press any key to return to the main menu...");
                Console.ReadKey();
            }
        }
    }

    // MENU DESIGN
    static void ShowMenu(List<Machine> machines)
    {
        // count machines that need attention
        int attention = 0;
        foreach (Machine m in machines)
        {
            string reminder = m.GetReminder();
            if (reminder.Contains("REPLACE") || reminder.Contains("LUBRICATE"))
            {
                attention++;
            }
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════╗");

        Console.Write("║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("        ⚙  R U S T   T R A C K E R  ⚙        ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("║");

        Console.Write("║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("         Machine Maintenance System         ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("║");

        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║                                            ║");

        Console.WriteLine("║   [+]  1. Add machine                      ║");
        Console.WriteLine("║   [!]  2. Show reminders                   ║");
        Console.WriteLine("║   [~]  3. Update last checked date         ║");
        Console.WriteLine("║   [x]  4. Exit                             ║");

        Console.WriteLine("║                                            ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");

        Console.Write("║");
        if (attention > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        string status = " Machines: " + machines.Count + "   Needs attention: " + attention;
        Console.Write(status.PadRight(44));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("║");

        Console.WriteLine("╚════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.Write("Choose (1-4): ");
    }

    static void AddMachine(List<Machine> machines)
    {
        Console.WriteLine("Type: 1 = CNC Machine, 2 = Generator, 3 = Air Compressor");
        Console.Write("Choose type: ");
        string type = Console.ReadLine();

        Machine m;
        if (type == "1")
        {
            m = new CncMachine();
        }
        else if (type == "2")
        {
            Generator g = new Generator();
            Console.Write("Is it outdoors? (y/n): ");
            g.IsOutdoor = Console.ReadLine() == "y";
            m = g;
        }
        else if (type == "3")
        {
            m = new AirCompressor();
        }
        else
        {
            Console.WriteLine("Invalid type.");
            return;
        }

        try
        {
            Console.Write("Name: ");
            m.Name = Console.ReadLine();
            Console.Write("Manufacture year (example 2020): ");
            m.ManufactureYear = int.Parse(Console.ReadLine());

            Console.Write("Date last checked (yyyy-mm-dd): ");
            m.LastChecked = DateTime.Parse(Console.ReadLine());
            Console.Write("Temperature where it is kept (C): ");
            double temp = double.Parse(Console.ReadLine());
            m.Temperature = temp;

            if (m.Temperature != temp)
            {
                Console.WriteLine("Machine not saved.");
                return;
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid input. Machine not saved.");
            return;
        }

        machines.Add(m);
        SaveAll(machines);
        Console.WriteLine("Saved to " + fileName);
    }

    // POLYMORPHISM: same call, different result per machine
    static void ShowReminders(List<Machine> machines)
    {
        if (machines.Count == 0)
        {
            Console.WriteLine("No machines yet.");
            return;
        }

        foreach (Machine m in machines)
        {
            Console.WriteLine(m.GetReminder());
        }
    }

    static void UpdateLastChecked(List<Machine> machines)
    {
        for (int i = 0; i < machines.Count; i++)
        {
            Console.WriteLine((i + 1) + ". " + machines[i].Name);
        }

        try
        {
            Console.Write("Machine number: ");
            int number = int.Parse(Console.ReadLine());
            Console.Write("New date last checked (yyyy-mm-dd): ");
            DateTime date = DateTime.Parse(Console.ReadLine());

            machines[number - 1].LastChecked = date;
            SaveAll(machines);
            Console.WriteLine("Updated.");
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid input. Nothing changed.");
        }
    }


    static void SaveAll(List<Machine> machines)
    {
        List<string> lines = new List<string>();
        foreach (Machine m in machines)
        {
            lines.Add(m.ToFileLine());
        }
        File.WriteAllLines(fileName, lines);
    }


    static List<Machine> LoadMachines()
    {
        List<Machine> machines = new List<Machine>();

        if (!File.Exists(fileName))
        {
            return machines;
        }

        string[] lines = File.ReadAllLines(fileName);
        foreach (string line in lines)
        {
            string[] parts = line.Split('|');
            Machine m;

            if (parts[0] == "CNC")
            {
                m = new CncMachine();
            }
            else if (parts[0] == "Generator")
            {
                Generator g = new Generator();
                g.IsOutdoor = bool.Parse(parts[5]);
                m = g;
            }
            else if (parts[0] == "Compressor")
            {
                m = new AirCompressor();
            }
            else
            {
                continue;
            }

            m.Name = parts[1];
            m.ManufactureYear = int.Parse(parts[2]);
            m.LastChecked = DateTime.Parse(parts[3]);
            m.Temperature = double.Parse(parts[4]);
            machines.Add(m);
        }

        return machines;
    }
}