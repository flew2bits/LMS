using MARC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages;

public class Index : PageModel
{
    [BindProperty] public IFormFile Marc { get; set; } = null!;

    public Record[] Records { get; set; } = Array.Empty<Record>();
    
    public void OnGet()
    {
    }

    public void OnPost()
    {
        var reader = new FileMARCReader(Marc.OpenReadStream());
        Records = reader.ToArray();
        
    }
}