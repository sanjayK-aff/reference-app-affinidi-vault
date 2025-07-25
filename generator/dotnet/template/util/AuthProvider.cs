using System;
using System.Threading.Tasks;
using Affinidi_Login_Demo_App.Util;

using Microsoft.Extensions.Logging;

namespace Affinidi_Login_Demo_App.Util
{
    public class IotaTokenOutput
    {
        public string IotaJwt { get; set; }
        public string IotaSessionId { get; set; }
    }

    public class AuthProviderParams
    {
        public string ProjectId { get; set; }
        public string TokenId { get; set; }
        public string KeyId { get; set; }
        public string PrivateKey { get; set; }
        public string Passphrase { get; set; }
        public string ApiGatewayUrl { get; set; }
        public string TokenEndpoint { get; set; }
    }

    public class AuthProvider
    {
        private string publicKey = string.Empty;
        private string projectScopedToken = string.Empty;
        private readonly string keyId;
        private readonly string tokenId;
        private readonly string passphrase;
        private readonly string privateKey;
        private readonly string projectId;
        private readonly string apiGatewayUrl;
        private readonly string tokenEndpoint;
        private readonly ProjectScopedToken _projectScopedTokenInstance;
        // Dummy Jwt and Iota classes for demonstration
        private readonly Jwt jwt;
        private readonly Iota iotaInstance;

        public AuthProvider(AuthProviderParams param)
        {
            apiGatewayUrl = param.ApiGatewayUrl ?? "";
            tokenEndpoint = param.TokenEndpoint ?? "https://dummy-token-endpoint";
            if (string.IsNullOrEmpty(param.PrivateKey) || string.IsNullOrEmpty(param.ProjectId) || string.IsNullOrEmpty(param.TokenId))
            {
                throw new ArgumentException("Missing parameters. Please provide privateKey, projectId and tokenId.");
            }
            projectId = param.ProjectId;
            tokenId = param.TokenId;
            keyId = param.KeyId ?? param.TokenId;
            privateKey = param.PrivateKey;
            passphrase = param.Passphrase;
            _projectScopedTokenInstance = new ProjectScopedToken();
            jwt = new Jwt();
            iotaInstance = new Iota();
        }

        private async Task<bool> ShouldRefreshToken()
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                publicKey = await jwt.FetchPublicKeyAsync(apiGatewayUrl);
            }
            bool itExistsAndExpired = !string.IsNullOrEmpty(projectScopedToken) && jwt.ValidateToken(projectScopedToken, publicKey).IsExpired;
            return string.IsNullOrEmpty(projectScopedToken) || itExistsAndExpired;
        }

        public async Task<string> FetchProjectScopedTokenAsync()
        {
             
            bool shouldRefreshToken = await ShouldRefreshToken();
            if (shouldRefreshToken)
            {
                projectScopedToken = await _projectScopedTokenInstance.FetchProjectScopedTokenAsync(apiGatewayUrl, projectId, tokenId, tokenEndpoint, privateKey, keyId, passphrase);
            }

           

            return projectScopedToken;
        }

        public IotaTokenOutput CreateIotaToken(string iotaConfigId, string did, string iotaSessionId = null)
        {
            string sessionId = iotaSessionId ?? Guid.NewGuid().ToString();
            return new IotaTokenOutput
            {
                IotaJwt = iotaInstance.SignIotaJwt(projectId, iotaConfigId, sessionId, keyId, tokenId, passphrase, privateKey, did),
                IotaSessionId = sessionId
            };
        }
    }

    // Dummy Jwt and Iota classes for demonstration
    public class Jwt
    {
        public async Task<string> FetchPublicKeyAsync(string apiGatewayUrl)
        {
            await Task.Delay(10); // Simulate async
            return "dummy_public_key";
        }
        public TokenValidationResult ValidateToken(string token, string publicKey)
        {
            // Simulate validation
            return new TokenValidationResult { IsExpired = false };
        }

        public class TokenValidationResult
        {
            public bool IsExpired { get; set; }
        }
    }

    public class Iota
    {
        public string SignIotaJwt(string projectId, string iotaConfigId, string sessionId, string keyId, string tokenId, string passphrase, string privateKey, string audience)
        {
            // Simulate JWT signing
            return $"dummy_iota_jwt_for_{sessionId}";
        }
    }
}
