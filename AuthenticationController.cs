
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(UserModel userModel)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7246/");
                var postTask = await client.PostAsJsonAsync<UserModel>("api/Authentication/login", userModel);
                if (postTask.IsSuccessStatusCode)
                {
                    var userjsonString =  postTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidationModel>(userjsonString.Result);
                    HttpContext.Session.SetString("Token", de.Token);
                    HttpContext.Session.SetString("userid", de.Id);
                    HttpContext.Session.SetString("Username", de.Username);
                    HttpContext.Session.SetString("UserRole", de.Role);
                    if(de.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }else if(de.Role == "User")
                    {
                        return RedirectToAction("Index", "Customer");
                    }
                }
                ModelState.AddModelError("", "Invalid Username or Password");
            }
            return View(userModel);
           
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            SignUpTemp user = new SignUpTemp();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpTemp model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7246/");
                    var postTask = await client.PostAsJsonAsync<SignUpTemp>("api/Authentication/signup", model);
                    if (postTask.IsSuccessStatusCode)
                    {
                        var userjsonString = postTask.Content.ReadAsStringAsync();
                        var de = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidationModel>(userjsonString.Result);
                        
                        return RedirectToAction("Login", "Authentication");
                    }
                    ModelState.AddModelError("", "Invalid Details");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            using(var client = new HttpClient())
            {
                HttpContext.Session.Clear();
            }
            return RedirectToAction("Login", "Authentication");
        }
    }
}

