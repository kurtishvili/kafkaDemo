// See https://aka.ms/new-console-template for more information

DateTime currentDate = DateTime.UtcNow.AddHours(8);
DateTime nextIntervalStart;

if (currentDate.AddMinutes(30) < new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0);
}
else if (currentDate.AddMinutes(30) >= new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 21, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0).AddDays(1);
}
else if (currentDate.AddMinutes(30) >= new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 18, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 21, 0, 0);
}
else if (currentDate.AddMinutes(30) >= new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 15, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 18, 0, 0);
}
else if (currentDate.AddMinutes(30) >= new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 15, 0, 0);
}
else if (currentDate.AddMinutes(30) >= new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0))
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 0, 0);
}
else
{
    nextIntervalStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0);
}

for (int i = 0; i < 5; i++)
{
    DateTime intervalEnd = nextIntervalStart.AddHours(3);
    Console.WriteLine($"{nextIntervalStart:HH:mm} - {intervalEnd:HH:mm}");

    nextIntervalStart = intervalEnd;

    if (nextIntervalStart.Hour >= 21)
    {
        nextIntervalStart = new DateTime(nextIntervalStart.Year, nextIntervalStart.Month, nextIntervalStart.Day, 9, 0, 0).AddDays(1);
    }
}