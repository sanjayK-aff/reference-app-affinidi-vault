using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Affinidi_Login_Demo_App.Util
{
    public class ProjectScopedToken
    {
        public string SignPayload(string tokenId, string audience, string privateKey, string keyId, string passphrase = null)
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

        // Dummy method to simulate fetching a project-scoped token
        public string FetchProjectScopedToken(string apiGatewayUrl, string projectId, string tokenId, string audience, string privateKey, string keyId, string passphrase = null)
        {
            // In a real implementation, you would use HttpClient to POST to the API Gateway
            // and return the token from the response. Here, we just simulate.
            var userAccessToken = SignPayload(tokenId, audience, privateKey, keyId, passphrase);
            // Simulate API call and return dummy token
            return userAccessToken;
        }
    }
}
