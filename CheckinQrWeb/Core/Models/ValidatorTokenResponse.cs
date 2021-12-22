using System.IdentityModel.Tokens.Jwt;

namespace CheckinQrWeb.Core.Models
{
    public class ValidatorTokenResponse
    {
        public ValidatorTokenResponse()
        {
          //  Errors = new List<string>();
        }
      ///  public List<string> Errors { get; set; }
      //  public bool IsValid => Errors == null || !Errors.Any();
        public string ValidationNonce { get; set; }
        public JwtSecurityToken AccessToken { get; set; }
        public string AccessTokenRaw { get; set; }
    }
}
