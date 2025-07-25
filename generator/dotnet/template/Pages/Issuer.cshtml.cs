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


        public void OnGet()
        {
            // Prefill handled by property initializers
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Read JSON body from AJAX
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            // Deserialize to dynamic object
            var formData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            GivenName = formData.ContainsKey("GivenName") ? formData["GivenName"]?.ToString() ?? "" : "";
            FamilyName = formData.ContainsKey("FamilyName") ? formData["FamilyName"]?.ToString() ?? "" : "";
            Email = formData.ContainsKey("Email") ? formData["Email"]?.ToString() ?? "" : "";

            if (formData.ContainsKey("Education") && formData["Education"] is System.Text.Json.JsonElement eduElem)
            {
                var eduDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(eduElem.GetRawText());
                Education.institutionName = eduDict.GetValueOrDefault("institutionName", "");
                Education.dateFrom = eduDict.GetValueOrDefault("dateFrom", "");
                Education.dateTo = eduDict.GetValueOrDefault("dateTo", "");
            }



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
                claimMode = ClaimModeEnum.NORMAL,
                holderDid = "did:key:zQ3shmB5BLKAgukNpe8e7TA93kuEGBgNjP5X6dDYvd1WyGzgT",
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
            var response = await credentialsClient.IssuanceStart(issuanceInput);
            // Return result as JSON
            return new JsonResult(response);
        }
    }
}
