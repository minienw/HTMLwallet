namespace CheckinQrWeb.Core.Models
{
    public class GetValidatorTokenRequest
    {
        public string ValidationServiceEndpoint { get; set; }
        public string AccessTokenServiceEndpoint { get; set; }
        public string QrValidationRequestJwtToken { get; set; }
        public string UserKey { get; set; }
    }
}
