using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace CheckinQrWeb.Core;

public class DccHelper
{
    public string PublicKey => _keyPair.Item1;
    public string PrivateKey => _keyPair.Item2;

    private readonly (string, string) _keyPair;
    public DccHelper()
    {
        _keyPair = GenerateRandomEcKeyPair();
    }
    public DccHelper(string publicKey, string privateKey)
    {
        _keyPair = new(publicKey, privateKey);
    }

    public byte[] EncryptDcc(string dcc, byte[] password, byte[] nonce)
    {
        var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform encryptor = aes.CreateEncryptor(password, nonce);

        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                    sw.Write(dcc);
                var encryptedDcc = ms.ToArray();
                return encryptedDcc;
            }
        }
    }

    public string SignDcc(byte[] dcc)
    {
        var privateKeyBase64 = _keyPair.Item2;
        var privateKey = PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKeyBase64));
        var signer = SignerUtilities.GetSigner(X9ObjectIdentifiers.ECDsaWithSha256.Id);
        signer.Init(true, privateKey);
        signer.BlockUpdate(dcc, 0, dcc.Length);

        var signature = signer.GenerateSignature();
        var signatureBase64 = Convert.ToBase64String(signature);
        return signatureBase64;
    }

    private (string, string) GenerateRandomEcKeyPair()
    {
        var curve = ECNamedCurveTable.GetByName("secp256k1");
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        var secureRandom = new SecureRandom();
        var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

        var generator = new ECKeyPairGenerator("ECDSA");
        generator.Init(keyParams);
        var keyPair = generator.GenerateKeyPair();

        var privateKey = keyPair.Private as ECPrivateKeyParameters;
        var publicKey = keyPair.Public as ECPublicKeyParameters;

        var pkInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
        var privateKeyBase64 = Convert.ToBase64String(pkInfo.GetDerEncoded());

        var info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
        var publicKeyBase64 = Convert.ToBase64String(info.GetDerEncoded());

        return (publicKeyBase64, privateKeyBase64);
    }
}