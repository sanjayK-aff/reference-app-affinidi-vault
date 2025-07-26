using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App.Util
{
    public enum ClaimModeEnum { NORMAL, TX_CODE, FIXED_HOLDER }
    // NOTE: The following classes are placeholders for the actual models from Affinidi's .NET SDKs.
    // Please replace them with the actual classes from the SDKs.
    public class StartIssuanceInput
    {

        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ClaimModeEnum claimMode { get; set; }
        public string? holderDid { get; set; }
        public List<CredentialData> data { get; set; } = new List<CredentialData>();
    }
    public class CredentialData
    {
        public string credentialTypeId { get; set; } = string.Empty;
        public object? credentialData { get; set; }
    }

    public class StartIssuanceResponse
    {
        [JsonPropertyName("credentialOfferUri")]
        public string? CredentialOfferUri { get; set; }

        [JsonPropertyName("issuanceId")]
        public string? IssuanceId { get; set; }

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("txCode")]
        public string? TxCode { get; set; }


    }

    public class IssuanceStatusResponse { }
    public class VerifyPresentationInput { }

    public class VerifyPresentationResponse { }

    public class ApiResponse<T> { public T? Data { get; set; } }

    public class IssuanceConfiguration { public required string BasePath { get; set; } }

    public class VerificationConfiguration { public required string BasePath { get; set; } }
    public class IssuanceApi
    {
        AuthProvider _authProvider;
        IssuanceConfiguration _config;
        public IssuanceApi(AuthProvider authProvider, IssuanceConfiguration config)
        {
            _authProvider = authProvider;
            _config = config;
        }
        public virtual async Task<ApiResponse<StartIssuanceResponse>> StartIssuanceAsync(string projectId, StartIssuanceInput input)
        {
            var localVarPath = $"cis/v1/{Uri.EscapeDataString(projectId)}/issuance/start";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            var token = await _authProvider.FetchProjectScopedTokenAsync();
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(input);



            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<StartIssuanceResponse>(responseBody);
                Console.WriteLine($"Issuance API response: {responseBody}");
                Console.WriteLine($"CredentialOfferUri: {data?.CredentialOfferUri}");
                Console.WriteLine($"IssuanceId: {data?.IssuanceId}");
                Console.WriteLine($"ExpiresIn: {data?.ExpiresIn}");
                Console.WriteLine($"TxCode: {data?.TxCode}");
                return new ApiResponse<StartIssuanceResponse> { Data = data };
            }
            Console.WriteLine($"Issuance API error: {response.StatusCode}");
            return new ApiResponse<StartIssuanceResponse> { Data = null };
        }
        public virtual Task<ApiResponse<IssuanceStatusResponse>> GetIssuanceStatusAsync(string issuanceId, string projectId) { throw new NotImplementedException(); }
    }

    // Custom DelegatingHandler to add the auth token to each request
    public class AuthHandler : DelegatingHandler
    {
        private readonly AuthProvider _authProvider;

        public AuthHandler(AuthProvider authProvider) : base(new HttpClientHandler())
        {
            _authProvider = authProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authProvider.FetchProjectScopedTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class CredentialsClient
    {
        private readonly IssuanceApi _issuanceApi;
        private readonly VerificationApi _verificationApi;
        private readonly AuthProviderParams _authProviderParams;

        public CredentialsClient()
        {

            _authProviderParams = new AuthProviderParams
            {
                ProjectId = System.Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                TokenId = System.Environment.GetEnvironmentVariable("TOKEN_ID") ?? string.Empty,
                KeyId = System.Environment.GetEnvironmentVariable("KEY_ID") ?? string.Empty,
                PrivateKey = System.Environment.GetEnvironmentVariable("PRIVATE_KEY") ?? string.Empty,
                Passphrase = System.Environment.GetEnvironmentVariable("PASSPHRASE") ?? string.Empty,
                ApiGatewayUrl = System.Environment.GetEnvironmentVariable("API_GATEWAY_URL") ?? string.Empty,
                TokenEndpoint = System.Environment.GetEnvironmentVariable("TOKEN_ENDPOINT") ?? string.Empty
            };
            AuthProvider authProvider = new AuthProvider(_authProviderParams);


            // Assuming SDK configuration objects
            var issuanceConfig = new IssuanceConfiguration { BasePath = $"{_authProviderParams.ApiGatewayUrl}/cis" };
            Console.WriteLine($"Issuance API Base Path: {issuanceConfig.BasePath}");
            _issuanceApi = new IssuanceApi(authProvider, issuanceConfig);

            var verificationConfig = new VerificationConfiguration { BasePath = $"{_authProviderParams.ApiGatewayUrl}/ver" };
            _verificationApi = new VerificationApi(authProvider, verificationConfig);
        }

        public async Task<StartIssuanceResponse> IssuanceStart(StartIssuanceInput apiData)
        {
            Console.WriteLine($"StartIssuanceAsync called with Project ID: {_authProviderParams.ProjectId}");
            var response = await _issuanceApi.StartIssuanceAsync(_authProviderParams.ProjectId, apiData);
            return response.Data;
        }

        public async Task<IssuanceStatusResponse> IssuanceStatus(string issuanceId)
        {
            var response = await _issuanceApi.GetIssuanceStatusAsync(issuanceId, _authProviderParams.ProjectId);
            return response.Data;
        }

        public async Task<VerifyPresentationResponse> VerifyPresentation(VerifyPresentationInput apiData)
        {
            var response = await _verificationApi.VerifyPresentationAsync(apiData);
            Console.WriteLine($"verifyPresentation response: {response.Data}");
            return response.Data;
        }
    }
    public class VerificationApi
    {
        public VerificationApi(AuthProvider authProvider, object config) { /* SDK Implementation */ }
        public virtual Task<ApiResponse<VerifyPresentationResponse>> VerifyPresentationAsync(VerifyPresentationInput input) { throw new NotImplementedException(); }
    }


}