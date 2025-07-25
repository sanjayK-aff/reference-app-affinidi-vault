using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Affinidi_Login_Demo_App.Util
{
    // NOTE: The following classes are placeholders for the actual models from Affinidi's .NET SDKs.
    // Please replace them with the actual classes from the SDKs.
    public class StartIssuanceInput
    {
        public enum ClaimModeEnum { Normal }
        public ClaimModeEnum ClaimMode { get; set; }
        public string HolderDid { get; set; }
        public List<CredentialData> Data { get; set; }
    }
    public class CredentialData
    {
        public string CredentialTypeId { get; set; }
        public object Credential { get; set; }
    }
    public class StartIssuanceResponse { /* Properties from SDK response */ }
    public class IssuanceStatusResponse { /* Properties from SDK response */ }
    public class VerifyPresentationInput { /* Properties from SDK request */ }
    public class VerifyPresentationResponse { /* Properties from SDK response */ }
    public class IssuanceApi
    {
        public IssuanceApi(HttpClient client, object config) { /* SDK Implementation */ }
        public virtual Task<ApiResponse<StartIssuanceResponse>> StartIssuanceAsync(string projectId, StartIssuanceInput input) { throw new NotImplementedException(); }
        public virtual Task<ApiResponse<IssuanceStatusResponse>> GetIssuanceStatusAsync(string issuanceId, string projectId) { throw new NotImplementedException(); }
    }
    public class VerificationApi
    {
        public VerificationApi(HttpClient client, object config) { /* SDK Implementation */ }
        public virtual Task<ApiResponse<VerifyPresentationResponse>> VerifyPresentationAsync(VerifyPresentationInput input) { throw new NotImplementedException(); }
    }
    public class ApiResponse<T> { public T Data { get; set; } }
    public class IssuanceConfiguration { public string BasePath { get; set; } }
    public class VerificationConfiguration { public string BasePath { get; set; } }

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
            var httpClient = new HttpClient(new AuthHandler(authProvider));

            // Assuming SDK configuration objects
            var issuanceConfig = new IssuanceConfiguration { BasePath = $"{apiGatewayUrl}/cis" };
            _issuanceApi = new IssuanceApi(httpClient, issuanceConfig);

            var verificationConfig = new VerificationConfiguration { BasePath = $"{apiGatewayUrl}/ver" };
            _verificationApi = new VerificationApi(httpClient, verificationConfig);
        }

        public async Task<StartIssuanceResponse> IssuanceStart(StartIssuanceInput apiData)
        {
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
}