using System.IdentityModel.Tokens.Jwt;

namespace CheckinQrWeb.Core.Models
{
    public class DccQrValidationRequest
    {
        public string DccQrData { get; set; }
        public string UserKey { get; set; }
        public ValidatorToken ValidatorToken { get; set; }
        public JwtSecurityToken JwtSecurityToken { get; set; }
        public string ValidationNonce { get; set; }
        public Publickeyjwk1 PublicKeyJwk { get; set; }
        public JwtSecurityToken ValidatorAccessToken { get; set; }
        public string ValidatorAccessTokenRaw { get; set; }
        public string PublicSigningKey { get; set; }
        public string PrivateSigningKey { get; set; }
    }
}
