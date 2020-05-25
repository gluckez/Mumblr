using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mumblr.Models;
using Mumblr.ViewModels;

namespace Mumblr.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IMumblrRepository mumblrRepository;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IMumblrRepository mumblrRepository,
                                 IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mumblrRepository = mumblrRepository;
            this.webHostEnvironment = webHostEnvironment;
        }

        
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {


                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }
                
                ModelState.AddModelError("", "Invalid Login Attempt");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };


                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, true);
                    return RedirectToAction("index", "home");
                }

                foreach(var error in result.Errors)
                {
                    // Shows in asp-validation-summary in the view
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            // Verify that its the users own profile
            var userVerify = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if(userVerify.Id == id)
            {
                var user = await userManager.FindByIdAsync(id);
                await signInManager.SignOutAsync();

                // Delete profilePicture
                if (user.PhotoPath != null)
                {
                    string uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    string profilePicture = Path.Combine(uploadFolder, user.PhotoPath);

                    try
                    {
                        System.IO.File.Delete(profilePicture);
                    }
                    catch (Exception e)
                    {

                    }
                }

                // Delete references in FriendSystem
                mumblrRepository.RemoveFriendSystemEntriesOfUser(id);

                // Remove messages of user
                mumblrRepository.removeMessagesOfDeletedProfiles(id);

                // Delete useraccount
                var result = await userManager.DeleteAsync(user);

                // Verify that the account has been deleted
                if (result.Succeeded)
                {
                    return RedirectToAction("register", "account");
                }
                else
                {
                    ViewBag.Errormessage = "There has been a problem deleting your account. Please contact the admin via Timo_Baader@hotmail.de";
                    return View("NotFound");
                }
            }
            else
            {
                ViewBag.Errormessage = "There profile you requested to delete does not seem to be your own.";
                return View("NotFound");
            }
        }
    }
}
