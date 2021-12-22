using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CheckinQrWeb.Core.Helpers
{
    public static class StringExtensions
    {
        public static string DecodeBase64(this string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }
        public static JwtSecurityToken? GetJwtSecurityToken(this string jwtTokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtTokenString);

            if (jsonToken == null) return null;

            var tokenS = jsonToken as JwtSecurityToken;
            return tokenS;
        }
        public static bool IsValidDccJson(this string dccQrJson)
        {
            return dccQrJson.StartsWith("HC1:");
        }
    }
}
