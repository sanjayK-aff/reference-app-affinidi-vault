using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Affinidi_Login_Demo_App.Util
{
    public class ProjectScopedToken
    {
        public string SignPayload(string tokenId, string audience, string privateKey, string keyId, string? passphrase = null)
        {
            var issueTimeInSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(privateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Iss, tokenId),
                new Claim(JwtRegisteredClaimNames.Sub, tokenId),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, (issueTimeInSeconds + 5 * 60).ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, issueTimeInSeconds.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Simulate POST to audience to get user access token
        public string GetUserAccessToken(string tokenId, string audience, string privateKey, string? passphrase, string keyId)
        {
            // Simulate JWT creation
            var jwt = SignPayload(tokenId, audience, privateKey, keyId, passphrase);

            // Simulate payload as per TS
            var input = new System.Collections.Generic.Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"scope", "openid"},
                {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"},
                {"client_assertion", jwt},
                {"client_id", tokenId}
            };

            // In a real implementation, use HttpClient to POST to 'audience' with input as x-www-form-urlencoded
            // Here, simulate by returning a dummy access token
            return $"dummy_access_token_{Guid.NewGuid()}";
        }

        // Simulate POST to API Gateway to get project-scoped token
        public string FetchProjectScopedToken(string apiGatewayUrl, string projectId, string tokenId, string audience, string privateKey, string keyId, string? passphrase = null)
        {
            // Step 1: Get user access token (simulate POST to audience)
            var userAccessToken = GetUserAccessToken(tokenId, audience, privateKey, passphrase, keyId);

            // Step 2: Simulate POST to API Gateway for project-scoped token
            var payload = new System.Collections.Generic.Dictionary<string, string>
            {
                {"projectId", projectId}
            };
            // In a real implementation, use HttpClient to POST to apiGatewayUrl/iam/v1/sts/create-project-scoped-token
            // with Authorization: Bearer userAccessToken and payload as JSON
            return $"dummy_project_scoped_token_for_{projectId}_with_{userAccessToken}";
        }
    }
}
