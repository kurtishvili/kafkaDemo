// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


FileStream fileStream = null;
try
{
    fileStream = new FileStream("example.txt", FileMode.Open);
    // Perform operations with the file
}
finally
{
    Console.WriteLine("asdadada");
    // Ensure the file stream is closed, even if an exception is thrown
    if (fileStream != null)
    {
        fileStream.Close();
    }
}