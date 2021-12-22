namespace CheckinQrWeb.Core.Models;

public class PublicKeyResponse
{
    public string id { get; set; }
    public Verificationmethod1[] verificationMethod { get; set; }
}

public class Verificationmethod1
{
    public string id { get; set; }
    public string type { get; set; }
    public string controller { get; set; }
    public Publickeyjwk1 publicKeyJwk { get; set; }
    public string[] verificationMethods { get; set; }
}

public class Publickeyjwk1
{
    public string[] x5c { get; set; }
    public string kid { get; set; }
    public string alg { get; set; }
    public string use { get; set; }
}
