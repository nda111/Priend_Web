using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PriendWeb.Pages.Account
{
    public class ResetModel : PageModel
    {
        public string Hash { get; set; } = null;

        public string Password { get; set; } = null;

        public void OnGet(string hash)
        {
            Hash = hash;
            Password = Request.Query["pw"];
        }
    }
}