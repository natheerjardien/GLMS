// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }
            [Required] [DataType(DataType.Password)] public string Password { get; set; }
            
            [Required] public string Name { get; set; }
            [Required] public string ContactDetails { get; set; }
            [Required] public string Region { get; set; }
        }

        // public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        //         var result = await _userManager.CreateAsync(user, Input.Password);

        //         if (result.Succeeded)
        //         {
        //             await _userManager.AddToRoleAsync(user, "Client");

        //             // creates the client profile linked to this user
        //             var newClient = new Client 
        //             { 
        //                 Name = Input.Name,
        //                 ContactDetails = Input.ContactDetails,
        //                 Region = Input.Region,
        //                 ApplicationUserId = user.Id
        //             };
                    
        //             _context.Clients.Add(newClient);
        //             await _context.SaveChangesAsync();

        //             await _signInManager.SignInAsync(user, isPersistent: false);
        //             return LocalRedirect(returnUrl ?? Url.Content("~/"));
        //         }
        //         foreach (var error in result.Errors)
        //         {
        //             ModelState.AddModelError(string.Empty, error.Description);
        //         }
        //     }
        //     return Page();
        // }
    }
}
