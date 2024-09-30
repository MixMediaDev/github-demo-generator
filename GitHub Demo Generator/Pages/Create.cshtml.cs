using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Octokit;

namespace GitHub_Demo_Generator.Pages
{
    public class CreateModel(GitHubClient client) : PageModel
    {
        private readonly GitHubClient _client = client;
        public List<string> Targets = new List<string>();

        public async Task<IActionResult> OnGet()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            _client.Credentials = new Credentials(token);

            await _client.Organization.GetAllForCurrent().ContinueWith(task =>
            {
                foreach (var org in task.Result)
                {
                    Targets.Add(org.Name);
                }
            });

            await _client.Repository.GetAllForCurrent().ContinueWith(task =>
            {
                foreach (var repo in task.Result)
                {
                    Targets.Add(repo.FullName);
                }
            });

            return Page();
        }
    }
}
