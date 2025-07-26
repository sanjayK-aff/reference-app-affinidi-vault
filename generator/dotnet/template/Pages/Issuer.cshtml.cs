using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App
{
    public class educationDetails
    {
        public string institutionName { get; set; } = "Example University";
        public string dateFrom { get; set; } = "2019";
        public string dateTo { get; set; } = "2023";
    }
    public class IssuerModel : PageModel
    {
        [BindProperty]
        public string GivenName { get; set; } = "John";
        [BindProperty]
        public string FamilyName { get; set; } = "Doe";
        [BindProperty]
        public string Email { get; set; } = "john.doe@example.com";
        [BindProperty]
        public educationDetails Education { get; set; } = new educationDetails();
        public bool IssuanceStarted { get; set; } = false;
        public bool IssuanceFinished { get; set; } = false;
        public string IssuanceResponseJson { get; set; } = "";
        public StartIssuanceResponse issuanceResponse { get; set; } = new StartIssuanceResponse();
        public string CredentialOfferUri { get; set; } = "";
        public string vaultUrl { get; set; } = Environment.GetEnvironmentVariable("PUBLIC_VAULT_URL") ?? "https://vault.affinidi.com";
        public string claimUrl { get; set; } = "";


        public void OnGet()
        {
            // Prefill handled by property initializers
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {


            var CredentialData = new
            {
                personalInformation = new
                {
                    firstName = GivenName,
                    lastName = FamilyName,
                    email = Email
                },
                educationDetails = new[]
                {
                    new Dictionary<string, string>
                    {
                        { "institutionName", Education.institutionName },
                        { "dateFrom", Education.dateFrom },
                        { "dateTo", Education.dateTo }
                    }
                }
            };

            var issuanceInput = new StartIssuanceInput
            {
                claimMode = ClaimModeEnum.TX_CODE,
                holderDid = "did:key:zQ3shZ5XvgFEiuLeBofUKk3QzHpEMpcfHYnPKVyDSdkKrkwqX",
                data = new List<CredentialData>
                {
                    new CredentialData
                    {
                        credentialTypeId = Environment.GetEnvironmentVariable("PUBLIC_CREDENTIAL_TYPE_ID") ?? string.Empty,
                        credentialData = CredentialData
                    }
                }
            };

            var credentialsClient = new CredentialsClient();
            issuanceResponse = await credentialsClient.IssuanceStart(issuanceInput);
            CredentialOfferUri = issuanceResponse?.CredentialOfferUri ?? "";
            IssuanceResponseJson = JsonConvert.SerializeObject(issuanceResponse);
            claimUrl = $"{vaultUrl}{Uri.EscapeDataString(CredentialOfferUri)}";
            Console.WriteLine($"Claim URL: {claimUrl}");
            IssuanceFinished = true;
            Console.WriteLine($"Issuance Response: {JsonConvert.SerializeObject(issuanceResponse)}");

            return Page();
        }
    }
}
