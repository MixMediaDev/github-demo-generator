using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Octokit;
using System.Security.Cryptography;

namespace GitHub_Demo_Generator.Pages;
public class IndexModel(ILogger<IndexModel> logger, GitHubClient client, IConfiguration config) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly GitHubClient _client = client;
    private readonly IConfiguration _config = config;

    private const string CSRFKEY = "CSRF:State";

    public async Task<IActionResult> OnGet()
    {
        if(Request.Query.ContainsKey("code"))
        {
            var code = Request.Query["code"].ToString();
            var clientId = config["GitHub:clientId"];
            var secret = config["GitHub:secret"];

            var token = await client.Oauth.CreateAccessToken(new OauthTokenRequest(clientId, secret, code));
            
            HttpContext.Session.SetString("AccessToken", token.AccessToken);
            _client.Credentials = new Credentials(token.AccessToken);

            var session = HttpContext.Session.GetString(CSRFKEY);
            var state = Request.Query["state"];

            if(!string.IsNullOrEmpty(session) && !string.Equals(session,state))
            {
                _logger.LogError(new InvalidOperationException("CSRF Attack Detected"), "X-Site Request Forgery detected, as State did not equal to original value");
            }
            HttpContext.Session.Remove(CSRFKEY);

            return RedirectToPage("Create");
        }
        return Page();
    }

    public IActionResult OnPostSignIn()
    {
        // Generate a CSRF token
        var csrfToken = GenerateCsrfToken();

        // Store the CSRF token in session
        HttpContext.Session.SetString(CSRFKEY, csrfToken);
        var clientId = _config["GitHub:clientId"];
        var returnUri = _config["GitHub:returnUri"];

        var request = new OauthLoginRequest(clientId)
        {
            Scopes = { "read:user", "repos", "admin:org", "workflow", "codespace", "write:packages", "project" },
            RedirectUri = new Uri(returnUri),
            State = csrfToken
        };

        // NOTE: user must be navigated to this URL
        var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);
        return Redirect(oauthLoginUrl.ToString());
    }

    private string GenerateCsrfToken()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
}
