using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Affinidi_Login_Demo_App.Util
{
    public enum ClaimModeEnum { Normal }
    // NOTE: The following classes are placeholders for the actual models from Affinidi's .NET SDKs.
    // Please replace them with the actual classes from the SDKs.
    public class StartIssuanceInput
    {

        public ClaimModeEnum ClaimMode { get; set; }
        public string? HolderDid { get; set; }
        public CredentialData Data { get; set; } = new CredentialData();
    }
    public class CredentialData
    {
        public string CredentialTypeId { get; set; } = string.Empty;
        public object? Credential { get; set; }
    }

    public class StartIssuanceResponse
    {

        public string CredentialOfferUri { get; set; } = string.Empty;

        public string? TxCode { get; set; }

        public string IssuanceId { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }
    }

    public class IssuanceStatusResponse { }
    public class VerifyPresentationInput { }

    public class VerifyPresentationResponse { }

    public class ApiResponse<T> { public T Data { get; set; } }

    public class IssuanceConfiguration { public required string BasePath { get; set; } }

    public class VerificationConfiguration { public required string BasePath { get; set; } }
    public class IssuanceApi
    {
        AuthProvider _authProvider;
        IssuanceConfiguration _config;
        public IssuanceApi(AuthProvider authProvider, IssuanceConfiguration config) {
            _authProvider = authProvider;
            _config = config;
        }
        public virtual Task<ApiResponse<StartIssuanceResponse>> StartIssuanceAsync(string projectId, StartIssuanceInput input)
        {
            
            var localVarPath = $"/v1/{Uri.EscapeDataString(projectId)}/issuance/start";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            Console.WriteLine($"Issuance API full URL: {fullUrl}");
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(input), System.Text.Encoding.UTF8, "application/json")
            };
            return new HttpClient().SendAsync(request)
                .ContinueWith(responseTask =>
                {
                    if (responseTask.Result.IsSuccessStatusCode)
                    {
                        var responseBody = responseTask.Result.Content.ReadAsStringAsync().Result;
                        var data = System.Text.Json.JsonSerializer.Deserialize<StartIssuanceResponse>(responseBody);
                        return new ApiResponse<StartIssuanceResponse> { Data = data };
                    }
                    return new ApiResponse<StartIssuanceResponse> { Data = null };
                });

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
        private readonly string _projectId;

        public CredentialsClient(AuthProvider authProvider, string apiGatewayUrl, string projectId)
        {
            _projectId = projectId;
            

            // Assuming SDK configuration objects
            var issuanceConfig = new IssuanceConfiguration { BasePath = $"{apiGatewayUrl}/cis" };
            Console.WriteLine($"Issuance API Base Path: {issuanceConfig.BasePath}");
            _issuanceApi = new IssuanceApi(authProvider, issuanceConfig);

            var verificationConfig = new VerificationConfiguration { BasePath = $"{apiGatewayUrl}/ver" };
            _verificationApi = new VerificationApi(authProvider, verificationConfig);
        }

        public async Task<StartIssuanceResponse> IssuanceStart(StartIssuanceInput apiData)
        {
            Console.WriteLine($"StartIssuanceAsync called with Project ID: {_projectId}");
            var response = await _issuanceApi.StartIssuanceAsync(_projectId, apiData);
            return response.Data;
        }

        public async Task<IssuanceStatusResponse> IssuanceStatus(string issuanceId)
        {
            var response = await _issuanceApi.GetIssuanceStatusAsync(issuanceId, _projectId);
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