using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Pedestal.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            string selectedSede = Request.Form["sede"];
            HttpContext.Session.SetString("CAF", selectedSede);
            return RedirectToPage("/Turno");
        }
    }
}
