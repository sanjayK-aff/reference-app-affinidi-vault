using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App
{
    public class IssuerModel : PageModel
    {
        [BindProperty]
        public string GivenName { get; set; } = "John";
        [BindProperty]
        public string FamilyName { get; set; } = "Doe";
        [BindProperty]
        public string Email { get; set; } = "john.doe@example.com";
        public bool IssuanceStarted { get; set; } = false;

        public void OnGet()
        {
            // Prefill handled by property initializers
        }

        public async Task OnPost()
        {
            AuthProviderParams authProviderParams = new AuthProviderParams
            {
                ProjectId = System.Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                TokenId = System.Environment.GetEnvironmentVariable("TOKEN_ID") ?? string.Empty,
                KeyId = System.Environment.GetEnvironmentVariable("KEY_ID") ?? string.Empty,
                PrivateKey = System.Environment.GetEnvironmentVariable("PRIVATE_KEY") ?? string.Empty,
                Passphrase = System.Environment.GetEnvironmentVariable("PASSPHRASE") ?? string.Empty,
                ApiGatewayUrl = System.Environment.GetEnvironmentVariable("API_GATEWAY_URL") ?? string.Empty,
                TokenEndpoint = System.Environment.GetEnvironmentVariable("TOKEN_ENDPOINT") ?? string.Empty
            };
            AuthProvider authProvider = new AuthProvider(authProviderParams);
            // var token = await authProvider.FetchProjectScopedTokenAsync();

            var personalInformation = new
            {
                firstName = GivenName,
                middleName = "",
                lastName = FamilyName,
                email = Email
            };

            string personalInformationJson = System.Text.Json.JsonSerializer.Serialize(new { personalInformation });
            Console.WriteLine(personalInformationJson);
            // Console.WriteLine($"Project Scoped Token: {token}");
            IssuanceStarted = true;
            var issuanceInput = new StartIssuanceInput
            {
                ClaimMode = ClaimModeEnum.Normal,
                HolderDid = "",
                Data = new CredentialData
                {
                    CredentialTypeId = Environment.GetEnvironmentVariable("PUBLIC_CREDENTIAL_TYPE_ID") ?? string.Empty,
                    Credential = personalInformation
                }
            };
            Console.WriteLine($"Issuance Input: {JsonConvert.SerializeObject(issuanceInput)}");
            // var issuanceApi = new IssuanceApi(new HttpClient(new AuthHandler(authProvider)), new IssuanceConfiguration { BasePath = authProviderParams.ApiGatewayUrl ?? string.Empty });
            // var response = await issuanceApi.StartIssuanceAsync(authProviderParams.ProjectId, issuanceInput);

            var credentialsClient = new CredentialsClient(authProvider, authProviderParams.ApiGatewayUrl, projectId: authProviderParams.ProjectId);
            var response = await credentialsClient.IssuanceStart(issuanceInput);
            Console.WriteLine($"Issuance Response: {response}");

        }
    }
}
