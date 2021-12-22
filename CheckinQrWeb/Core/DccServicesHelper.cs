using System;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using CheckinQrWeb.Core;
using System.Text;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using CheckinQrWeb.Core.Helpers;
using CheckinQrWeb.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace CheckinQrWeb.Core
{
    public class DccServicesHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private DccHelper _dccHelper;

        public DccServicesHelper(IHttpClientFactory httpClientFactory, DccHelper dccHelper)
        {
            _httpClientFactory = httpClientFactory;
            _dccHelper = dccHelper;
        }

        public QrValidationRequestProcessingResult ProcessStartDccValidationRequest(string jsonRequest)
        {
            var result = new QrValidationRequestProcessingResult();
            if (string.IsNullOrWhiteSpace(jsonRequest))
            {
                throw new ArgumentException("Request could not be processed: empty request");
            }

            var requestModel = JsonConvert.DeserializeObject<QrValidationRequest>(jsonRequest);
            if (requestModel == null)
            {
                throw new Exception($"Request could not be processed due to deserialization error");
            }

            result.QrValidationRequest = requestModel;

            var jwtToken = result.QrValidationRequest?.Token.GetJwtSecurityToken();
            if (jwtToken == null)
            {
                throw new Exception(
                    "Request could not be processed due to deserialization error: JWT token is invalid");
            }

            result.JwtIssToken = jwtToken;

            // generate keypair
            result.PrivateKey = _dccHelper.PrivateKey;
            result.PublicKey = _dccHelper.PublicKey;

            return result;
        }

        public async Task<ValidationServiceDescriptionData> GetValidationServiceDescriptionData(
            ValidationServiceDescriptionDataRequest request)
        {
            var result = new ValidationServiceDescriptionData();
            if (string.IsNullOrWhiteSpace(request.VerificationServiceIdentity))
            {
                throw new Exception($"Request could not be processed: empty request");
            }

            result.VerificationServiceDescription =
                await GetVerificationServiceDescription(request.VerificationServiceIdentity);
            result.ValidationService1 = GetValidationService1(result.VerificationServiceDescription);
            result.AccessTokenService1 = GetAccessTokenService1(result.VerificationServiceDescription);
            result.PublicKeyJwk = await GetPublicKeyFromValidationService(result.ValidationService1.serviceEndpoint);

            return result;
        }

        private async Task<VerificationServiceDescription> GetVerificationServiceDescription(string serviceIdentity)
        {
            var client = _httpClientFactory.CreateClient();

            var idSvcRequest = await client.GetAsync(serviceIdentity);
            var jsonData = await idSvcRequest.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                throw new Exception(
                    $"Getting the validation service description data failed: no valid data received from service with url {serviceIdentity}");
            }

            var result = JsonConvert.DeserializeObject<VerificationServiceDescription>(jsonData);
            if (result == null)
            {
                throw new Exception(
                    $"Deserializing the validation service description data failed: no valid data received from service with url {serviceIdentity}");
            }

            return result;
        }

        private Service GetValidationService1(VerificationServiceDescription verificationServiceDescription)
        {
            var service = (from x in verificationServiceDescription.service
                           where x.id.EndsWith("#ValidationService-1")
                           select x).SingleOrDefault();

            if (service == null)
            {
                throw new Exception($"Cannot retrieve ValidationService-1");
            }

            return service;
        }

        private Service GetAccessTokenService1(VerificationServiceDescription verificationServiceDescription)
        {
            var service = (from x in verificationServiceDescription.service
                           where x.id.EndsWith("#AccessTokenService-1")
                           select x).SingleOrDefault();

            if (service == null)
            {
                throw new Exception($"Cannot retrieve AccessTokenService-1");
            }

            return service;
        }

        private async Task<Publickeyjwk1> GetPublicKeyFromValidationService(string validationServiceEndpoint)
        {
            var identityUrl = $"{validationServiceEndpoint}/identity";
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponse = await httpClient.GetAsync(identityUrl);
            var content = await httpResponse.Content.ReadAsStringAsync();

            var model = JsonConvert.DeserializeObject<PublicKeyResponse>(content);

            if (model == null)
            {
                throw new Exception("Error deserializing PublicKeyResponse");
            }

            var result =
                (from method in model.verificationMethod
                 where method.id.EndsWith("ValidationServiceEncKey-1")
                 select method.publicKeyJwk).SingleOrDefault();

            if (result == null)
            {
                throw new Exception("Cannot retrieve ValidationServiceEncKey-1");
            }

            return result;
        }

        public async Task<ValidatorTokenResponse> GetValidatorToken(GetValidatorTokenRequest request)
        {
            var result = new ValidatorTokenResponse();

            if (string.IsNullOrWhiteSpace(request.UserKey))
            {
                throw new ArgumentException("Unable to get validator token response: required userkey is empty");
            }

            if (string.IsNullOrWhiteSpace(request.AccessTokenServiceEndpoint))
            {
                throw new ArgumentException(
                    "Unable to get validator token response: required access token service endpoint is empty");
            }

            if (string.IsNullOrWhiteSpace(request.ValidationServiceEndpoint))
            {
                throw new ArgumentException(
                    "Unable to get validator token response: required validation service endpoint is empty");
            }

            var requestData = new StringContent(
                JsonConvert.SerializeObject(new AccessTokenRequest
                {
                    pubKey = request.UserKey,
                    service = request.ValidationServiceEndpoint
                }),
                Encoding.UTF8,
                "application/json");

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                request.AccessTokenServiceEndpoint)
            {
                Headers =
                {
                    {
                        HeaderNames.Authorization,
                        new AuthenticationHeaderValue("Bearer", request.QrValidationRequestJwtToken).ToString()
                    },
                    {"X-Version", "1.0"}
                },
                Content = requestData
            };

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponse = await httpClient.SendAsync(httpRequestMessage);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unable to get validator token response. QR code might be invalid or expired. Http response code: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
            }

            var nonce = httpResponse.Headers.GetValues("x-nonce").FirstOrDefault();

            if (nonce == null)
            {
                throw new Exception(
                    "Unable to get validator token response: required nonce not found in response headers.");
            }

            result.ValidationNonce = nonce;
            var content = await httpResponse.Content.ReadAsStringAsync();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(content);
            result.AccessTokenRaw = content;
            var accessToken = jsonToken as JwtSecurityToken;
            if (accessToken == null)
            {
                throw new Exception("Unable to get validator token response: no valid security token received");
            }

            result.AccessToken = accessToken;
            return result;
        }

        public async Task<DccQrValidationResult> SubmitDccQrValidationRequest(DccQrValidationRequest request)
        {
            var result = new DccQrValidationResult();
            _dccHelper = new DccHelper(request.PublicSigningKey, request.PrivateSigningKey);

            var password = RandomNumberGenerator.GetBytes(32);

            var nonce = Convert.FromBase64String(request.ValidationNonce);
            var hcert = request.DccQrData;
            var encryptedDcc = _dccHelper.EncryptDcc(hcert, password, nonce);

            //x5c claim (public key of the certificate) 
            var x5cKey = request.PublicKeyJwk.x5c[0];
            var pemBytes = Convert.FromBase64String(x5cKey);
            var c = new X509Certificate2(pemBytes);
            var rsa = c.GetRSAPublicKey();
            var encPassword = rsa.Encrypt(password, RSAEncryptionPadding.OaepSHA256);
            var signatureBase64 = _dccHelper.SignDcc(encryptedDcc);

            var body = new
            {
                kid = request.PublicKeyJwk.kid,
                dcc = Convert.ToBase64String(encryptedDcc),
                sig = signatureBase64,
                encKey = Convert.ToBase64String(encPassword),
                encScheme = "RSAOAEPWithSHA256AESCBC",
                sigAlg = "SHA256withECDSA"
            };

            var endpoint = request.ValidatorAccessToken.Payload["aud"].ToString();

            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint)
            {
                Headers =
                {
                    {
                        HeaderNames.Authorization,
                        new AuthenticationHeaderValue("Bearer", request.ValidatorAccessTokenRaw).ToString()
                    },
                    {"X-Version", "1.0"}
                },
                Content = new StringContent(JsonConvert.SerializeObject(body)
                    , Encoding.UTF8, "application/json")
            };

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponse = await httpClient.SendAsync(httpRequestMessage);
            var content = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Http response code: {httpResponse.StatusCode}, message: {content}");
            }

            result.Content = content;

            return result;
        }

        public DccJsonTokenResult AnalyseDccJsonToken(SecurityToken jsonToken)
        {
            var result = new DccJsonTokenResult();

            if (jsonToken == null)
            {
                result.Errors.Add($"Empty security token received from DCC validation service.");
                return result;
            }

            var token = jsonToken as JwtSecurityToken;
            // verify end result
            var resultClaim = token.Claims.First(x => x.Type == "result");
            var resultsClaim = token.Claims.Where(x => x.Type == "results").ToList();
            if (resultClaim == null)
            {
                result.Errors.Add($"Empty security token received from DCC validation service.");
                return result;
            }

            if (resultClaim.Value.Equals("NOK", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Errors.Add($"Security token received from DCC validation service with result NOK. Review the error messages for details.");

                if (resultsClaim != null && resultsClaim.Count > 0)
                {
                    foreach (var results in resultsClaim)
                    {
                        result.Errors.Add(results.ToString());
                    }
                }

                return result;
            }

            return result;
        }
    }
}