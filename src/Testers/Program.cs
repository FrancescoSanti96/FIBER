using System.Reflection;
using Testers.Tests;

var testers = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => typeof(ITest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .Select(t => (ITest)Activator.CreateInstance(t)!)
    .ToList();

while (true)
{
    Console.WriteLine("\n== Tester disponibili ==");
    for (int i = 0; i < testers.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {testers[i].Name}");
    }
    Console.WriteLine("0. Esci");

    Console.Write("\nScegli un'opzione: ");
    if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > testers.Count)
    {
        Console.WriteLine("Scelta non valida!");
        continue;
    }

    if (choice == 0)
        break;

    Console.Clear();
    testers[choice - 1].Run();
    Console.WriteLine("\nPremi INVIO per continuare...");
    Console.ReadLine();
    Console.Clear();
}
