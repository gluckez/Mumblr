using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mumblr.Models;
using Mumblr.ViewModels;

namespace Mumblr.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMumblrRepository mumblrRepository;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IWebHostEnvironment webHostEnvironment,
                              IMumblrRepository mumblrRepository)
        {
            _logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.webHostEnvironment = webHostEnvironment;
            this.mumblrRepository = mumblrRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (signInManager.IsSignedIn(User))
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                return RedirectToAction("showProfile", "Home", new { Id = user.Id });
            }
            else
            {
                return RedirectToAction("register", "account");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> AddFriend(string id)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            mumblrRepository.AddFriend(user.Id, id);

            return RedirectToAction("showProfile", "Home", new { Id = id });
        }

        public async Task<IActionResult> RemoveFriend(string id)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            mumblrRepository.RemoveFriend(user.Id, id);

            return RedirectToAction("showProfile", "Home", new { Id = id });
        }

        

        [HttpGet]
        public IActionResult ListUsers()
        {
            var model = new List<ApplicationUser>();

            foreach (var user in userManager.Users)
            {
                model.Add(user);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ShowProfile(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user != null)
            {
                var model = new ShowProfileViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Bio = user.Bio,
                    PhotoPath = user.PhotoPath,
                    Feed = mumblrRepository.getFeed(id)    
                };

                return View(model);
            }
            else
            {
                ViewBag.ErrorMessage = "The requested profile could not be found. It may have been deleted";
                return View("NotFound");
            }
        }

        public async Task<IActionResult> deletePost(int id)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            // Verify that the user himself created that post
            if(mumblrRepository.verifyItIsUsersOwnPost(id, user.Id))
            {
                var success = mumblrRepository.deleteUserPost(id);
                // Verify that the database could handle the request
                if (success)
                {
                    return RedirectToAction("showProfile", "Home", new { user.Id });
                }
                else
                {
                    ViewBag.ErrorMessage = "The post you requested to delete does either not exist or cannot be deleted. You may contact the admin via Timo_Baader@hotmail.de";
                    return View("NotFound");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "The post you requested to delete does not seem to belong to you.";
                return View("NotFound");
            }


        }

        public async Task<IActionResult> deleteMessage(int id)
        {
            // Get the recipient of that message to redirect to that same chatWindow
            var recipient = mumblrRepository.returnMessageRecipient(id);

            // Verify that the user himself sent that message
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            if(mumblrRepository.verifyItIsUsersOwnMessage(id, user.Id))
            {
                // Delete the message
                var success = mumblrRepository.deleteUserMessage(id);

                // Verify that the database could handle the request
                if (success)
                {
                    return RedirectToAction("Inbox", "Home", new { id = recipient });
                }
                else
                {
                    ViewBag.ErrorMessage = "The message you requested to delete does either not exist or cannot be deleted. You may contact the admin via Timo_Baader@hotmail.de";
                    return View("NotFound");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "The message you requested to delete does not seem to belong to you.";
                return View("NotFound");
            }


        }

        // Called when a post is submitted
        [HttpPost]
        public async Task<IActionResult> ShowProfile(ShowProfileViewModel model)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (ModelState.IsValid)
            {
                // Create a new post 
                var newPost = new Post
                {
                    ApplicationUser = user,
                    Date = DateTime.Now,
                    Message = model.PostMessage,
                    Title = model.PostTitle
                };

                mumblrRepository.addPost(newPost);

                return RedirectToAction("showProfile", "Home", new { user.Id });
            }

            else
            {
                model.Bio = user.Bio;
                model.Feed = mumblrRepository.getFeed(user.Id);
                model.PhotoPath = user.PhotoPath;
                model.UserId = user.Id;
                model.UserName = user.UserName;

                return View(model);

            }
                     
        }

  

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {

            var user = await userManager.FindByIdAsync(id);

            if (user.UserName == HttpContext.User.Identity.Name)
            {
                var model = new EditUserViewModel()
                {
                    Name = user.UserName,
                    Age = user.Age,
                    Bio = user.Bio,
                    Email = user.Email,

                };

                return View(model);
            }
            else
            {
                ViewBag.Errormessage = "User could not be found";
                return View("NotFound");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            string uniqueFileName = null;

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if(model.Photo != null)
                {
                    string uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    // Check for an existing profilepicture and delete it
                    if (user.PhotoPath != null)
                    {
                        string oldPhoto = Path.Combine(uploadFolder, user.PhotoPath);

                        try
                        {
                            System.IO.File.Delete(oldPhoto);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    user.PhotoPath = uniqueFileName;
                }
                


                user.UserName = model.Name;
                user.Age = model.Age;
                user.Bio = model.Bio;
                

                await userManager.UpdateAsync(user);

                return RedirectToAction("showProfile", "home", new { user.Id });
            }

            else
            {
                return View(model);
            }
        }

        public async  Task<IActionResult> likePost(int id, string currentProfile)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var success = mumblrRepository.likePost(id, user.Id);

            // Verify that the database could handle the request
            if (success)
            {
                return RedirectToAction("showProfile", "home", new { id = currentProfile });
            }
            else
            {
                ViewBag.ErrorMessage = "Request could not be handled. You may contact the admin via Timo_Baader@hotmail.de";
                return View("NotFound");
            }
        }

        public async Task<IActionResult> unLikePost(int id, string currentProfile)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var success = mumblrRepository.unLikePost(id, user.Id);

            // Verify that the database could handle the request
            if (success)
            {
                return RedirectToAction("showProfile", "home", new { id = currentProfile });
            }
            else
            {
                ViewBag.ErrorMessage = "Request could not be handled. You may contact the admin via Timo_Baader@hotmail.de";
                return View("NotFound");
            }
        }

        // Handles the new chatMessages passed by the users
        [HttpPost]
        public async Task<IActionResult> Inbox(InboxViewModel model)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            // Feed the model
            model.UserFriends = mumblrRepository.getUserFriends(user.Id);
            model.SelectedFriend = model.NewChatMessage.recipient;
            model.Chat = mumblrRepository.getChat(user.Id, model.NewChatMessage.recipient);

            // Verify that the user entered a message
            if (model.NewChatMessage.Message != null)
            {
                var success = mumblrRepository.addChatMessage(user.Id, model.NewChatMessage.recipient, model.NewChatMessage.Message);
                // Verify that the database could handle the request
                if (success)
                {
                    return RedirectToAction("Inbox", "home", new { model.NewChatMessage.recipient });
                }
                // Errormessage in case the database cant handle the request
                else
                {
                    ViewBag.ErrorMessage = "There was an error processing the message. You may contact the admin via Timo_Baader@hotmail.de";
                    return View("NotFound");
                }
            }
            // Errormessage in case the user has not entered a valid message (Inputvalidation)
            else
            {
                return View(model);
            }
        }

        // If friendId is passed, get the user's and friend's respective chat
        [HttpGet]
#nullable enable
        public async Task<IActionResult> Inbox(string? id)
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            // If the user clicked on a friend to open their chat
            if (id != null && user.Id != null)
            {
                
                var model = new InboxViewModel()
                {
                    Chat = mumblrRepository.getChat(user.Id, id),
                    UserFriends = mumblrRepository.getUserFriends(user.Id),
                    SelectedFriend = id
                };

                // Mark the messages as read
                var success = mumblrRepository.markMessagesAsRead(user.Id, id);

                // Verify that the database could handle the request
                if (success)
                {
                    return View(model);
                }
                else
                {
                    ViewBag.ErrorMessage = "There was an error processing the message. You may contact the admin via Timo_Baader@hotmail.de";
                    return View("NotFound");
                }


            }

            // If the user has not clicked on a friend to open their chat yet
            else if (user.Id != null)
            {
                var model = new InboxViewModel()
                {
                    UserFriends = mumblrRepository.getUserFriends(user.Id),
                };
                return View(model);
            }
            ViewBag.ErrorMessage = "Request could not be handled. You may contact the admin via Timo_Baader@hotmail.de";
            return View("NotFound");
        }
    }
}
