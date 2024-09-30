using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHub_Demo_Generator.Pages
{
    public class ExtractModel : PageModel
    {
        public List<string> Organizations = new List<string>();
        public List<string> Repositories = new List<string>();
        public object AnalysisResult = null;

        public void OnGet()
        {
        }
    }
}
