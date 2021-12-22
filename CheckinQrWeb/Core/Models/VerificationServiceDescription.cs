namespace CheckinQrWeb.Core.Models
{
    public class VerificationServiceDescription
    {
        public string id { get; set; }
        public Verificationmethod[] verificationMethod { get; set; }
        public Service[] service { get; set; }
    }
    public class Verificationmethod
    {
        public string id { get; set; }
        public string type { get; set; }
        public string controller { get; set; }
        public Publickeyjwk publicKeyJwk { get; set; }
    }
    public class Publickeyjwk
    {
        public string[] x5c { get; set; }
        public string kid { get; set; }
        public string alg { get; set; }
        public string use { get; set; }
    }
    public class Service
    {
        public string id { get; set; }
        public string type { get; set; }
        public string serviceEndpoint { get; set; }
        public string name { get; set; }
    }

}