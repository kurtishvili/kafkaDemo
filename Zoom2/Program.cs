// See https://aka.ms/new-console-template for more information

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

Console.WriteLine("Hello, World!");

string sdkKey = "FdygKUT2R1yIIyYI0vWtxQ";
string sdkSecret = "UcLpWyval37IRhlEL1in8874rgGDsLPaV77L";
string sessionName = "maestro_morisiii";
string userIdentity = "tablesito";


string signature = GenerateSignature(sdkKey, sdkSecret, sessionName, userIdentity);

Console.WriteLine(signature);


static string GenerateSignature(string sdkKey, string sdkSecret, string sessionName, string userIdentity)
{
    var apiKey = "I4zr_wZXTkOrMR6nLJ8XqA";

    var iat = DateTimeOffset.UtcNow.AddSeconds(-30).ToUnixTimeSeconds();
    var exp = iat + 60 * 60 * 2;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sdkSecret));
    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new SecurityTokenDescriptor
    {
        Claims = new Dictionary<string, object>()
        {
            {"app_key", sdkKey},
            {"role_type", 1},
            {"tpc", sessionName},
            {"iss", apiKey},
            {"user_identity", userIdentity},
            {"version", "1"},
            {"iat", iat},
            {"exp", exp},
        },
        SigningCredentials = signingCredentials
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var t = tokenHandler.CreateToken(token);

    return tokenHandler.WriteToken(t);

}
