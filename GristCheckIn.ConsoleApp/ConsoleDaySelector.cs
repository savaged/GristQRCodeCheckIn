using GristCheckIn.Core.CheckIn;

namespace GristCheckIn.ConsoleApp;

/// <summary>
/// Console implementation of IDaySelector. A WPF app would provide its own
/// (e.g. a dialog with buttons) - Core doesn't need to know or care.
/// </summary>
public class ConsoleDaySelector : IDaySelector
{
    public string SelectColumn(IReadOnlyDictionary<DayOfWeek, string> options)
    {
        Console.WriteLine("Today isn't one of the configured check-in days.");

        var order = new[] { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
        var choices = order.Where(options.ContainsKey)
                            .Select(d => (Day: d, Column: options[d]))
                            .ToList();

        for (int i = 0; i < choices.Count; i++)
            Console.WriteLine($"  {i + 1}. {choices[i].Day} -> {choices[i].Column}");

        while (true)
        {
            Console.Write("Select day number: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= choices.Count)
                return choices[choice - 1].Column;
            Console.WriteLine("Invalid choice, try again.");
        }
    }
}
