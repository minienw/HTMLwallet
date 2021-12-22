namespace CheckinQrWeb.Core.Models
{
    public class ValidationServiceDescriptionData
    {
        public ValidationServiceDescriptionData()
        {
         //   Errors = new List<string>();
        }
       // public List<string> Errors { get; set; }
        public VerificationServiceDescription VerificationServiceDescription { get; set; }
        public Service ValidationService1 { get; set; }
        public Service AccessTokenService1 { get; set; }
        public Publickeyjwk1 PublicKeyJwk { get; set; }
      //  public bool IsValid => Errors == null || !Errors.Any();
    }
}
