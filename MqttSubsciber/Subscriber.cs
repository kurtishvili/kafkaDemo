var text = "redder";

var secondString = new string (text.ToCharArray().Reverse().ToArray());

if(text == secondString)
{
    Console.WriteLine(true);
}
else
{
    Console.WriteLine(false);
}

