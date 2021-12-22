using System.IdentityModel.Tokens.Jwt;

namespace CheckinQrWeb.Core.Models
{
    public class QrValidationRequestProcessingResult
    {
        public QrValidationRequestProcessingResult()
        {
        //    Errors = new List<string>();
        }
       // public List<string> Errors { get; set; }
      //  public bool IsValid => Errors == null || !Errors.Any();
        public QrValidationRequest QrValidationRequest { get; set; }
        public JwtSecurityToken JwtIssToken { get; set; }
        public string PrivateKey { get; internal set; }
        public string PublicKey { get; internal set; }
    }
}
