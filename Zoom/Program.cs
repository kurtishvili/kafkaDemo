// See https://aka.ms/new-console-template for more information

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

Console.WriteLine("Hello, World!");

var sdkKey = "FdygKUT2R1yIIyYI0vWtxQ";
var sdkSecret = "UcLpWyval37IRhlEL1in8874rgGDsLPaV77L";
var apiKey = "I4zr_wZXTkOrMR6nLJ8XqA";
var apiSecret = "6tyA4ifxf597mBGuPfD0PvSUi9oGEFnMm5mN";

var iat = DateTimeOffset.UtcNow.AddSeconds(-30).ToUnixTimeSeconds();
var exp = iat + 60 * 60 * 2;

var header = new
{
    alg = "HS256",
    typ = "JWT"
};


var dictionary = new Dictionary<string, object>()
{
    {"app_key", sdkKey},
    {"role_type", 0},
    {"tpc", "maestro_morisiii"},
    {"user_identity", "kurtisha"},
    {"version", "1"},
    {"iat", iat},
    {"exp", exp},
    {"iss", apiKey}
};

var token = Create(sdkSecret, apiKey, dictionary);

Console.WriteLine(token);


string Create(string secret, string issuer, Dictionary<string, object> dictionary)
{
    var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var signingCredentials = new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256Signature);
    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

    dictionary.TryAdd(JwtRegisteredClaimNames.Iss, issuer);
    dictionary.TryAdd(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"));

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Claims = dictionary,
        SigningCredentials = signingCredentials
    };

    var token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);

    return jwtSecurityTokenHandler.WriteToken(token);
}
