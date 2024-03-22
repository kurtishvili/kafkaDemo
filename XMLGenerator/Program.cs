// See https://aka.ms/new-console-template for more information

var visa = "4***********1593";

var amex = "3**********2106";

var mastercard = "5***********2563";

var maskedCardNumber = visa;



if (string.IsNullOrEmpty(maskedCardNumber) || maskedCardNumber.Length < 15)
{
    Console.WriteLine(6);
}

char firstChar = maskedCardNumber[0];
string lastFourDigits = maskedCardNumber.Substring(maskedCardNumber.Length - 4);

// Check the first character and last four digits
if (firstChar == '4')
{
    Console.WriteLine(1);
}
else if (firstChar == '5')
{
    Console.WriteLine(2);
}
else if (firstChar == '3' && (lastFourDigits == "6011" || lastFourDigits.StartsWith("65")))
{
    Console.WriteLine(3);
}
else if (firstChar == '3' && (lastFourDigits == "2011" || lastFourDigits == "1800" || lastFourDigits.StartsWith("35")))
{
    Console.WriteLine(4);
}
else if (firstChar == '3' && (lastFourDigits.StartsWith("34") || lastFourDigits.StartsWith("37")))
{
    Console.WriteLine(5);
}
else
{
    Console.WriteLine(6);
}


//var cardNumber = "5***********9439";

//var numericCardMask = Regex.Replace(cardNumber.Replace(" ", ""), "[^0-9]", "0");

//if (Regex.Match(numericCardMask, @"^4[0-9]{12}(?:[0-9]{3})?$").Success)
//    Console.WriteLine(1);
//else if (Regex.Match(numericCardMask, @"^5[1-5][0-9]{14}|^(222[1-9]|22[3-9]\d|2[3-6]\d{2}|27[0-1]\d|2720)[0-9]{12}$").Success)
//    Console.WriteLine(2);
//else if (Regex.Match(numericCardMask, @"^3[47][0-9]{13}$").Success)
//    Console.WriteLine(3);
//else if (Regex.Match(numericCardMask, @"^6(?:011|5[0-9]{2})[0-9]{12}$").Success)
//    Console.WriteLine(4);
//else if (Regex.Match(numericCardMask, @"^(?:2131|1800|35\d{3})\d{11}$").Success)
//    Console.WriteLine(5);
//else
//    Console.WriteLine(6);


