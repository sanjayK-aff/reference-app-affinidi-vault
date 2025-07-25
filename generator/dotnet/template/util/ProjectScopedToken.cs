using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App.Util
{
    public class ProjectScopedToken
    {
        public string SignPayload(string tokenId, string audience, string privateKey, string keyId, string? passphrase)
        {
            var issueTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var rsa = RSA.Create();
            if (!string.IsNullOrEmpty(passphrase))
            {
                rsa.ImportFromEncryptedPem(privateKey, passphrase);
            }
            else
            {
                rsa.ImportFromPem(privateKey);
            }

            var securityKey = new RsaSecurityKey(rsa) { KeyId = keyId };
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var header = new JwtHeader(credentials);

            var payload = new JwtPayload
            {
                { "iss", tokenId },
                { "sub", tokenId },
                { "aud", audience },
                { "jti", Guid.NewGuid().ToString() },
                { "exp", issueTimeInSeconds + 5 * 60 },
                { "iat", issueTimeInSeconds }
            };

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GetUserAccessTokenAsync(string tokenId, string audience, string privateKey, string? passphrase, string keyId)
        {
            var jwt = SignPayload(tokenId, audience, privateKey, keyId, passphrase);

            var input = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"scope", "openid"},
                {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"},
                {"client_assertion", jwt},
                {"client_id", tokenId}
            };

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, audience)
            {
                Content = new FormUrlEncodedContent(input)
            };

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            return tokenResponse?["access_token"];
        }

        public async Task<string> FetchProjectScopedTokenAsync(string apiGatewayUrl, string projectId, string tokenId, string audience, string privateKey, string keyId, string? passphrase)
        {
            var userAccessToken = await GetUserAccessTokenAsync(tokenId, audience, privateKey, passphrase, keyId);

            using var httpClient = new HttpClient();
            var requestUrl = $"{apiGatewayUrl}/iam/v1/sts/create-project-scoped-token";
            var payload = new { projectId };
            var jsonPayload = JsonConvert.SerializeObject(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            return tokenResponse?["accessToken"];
        }
    }
}
