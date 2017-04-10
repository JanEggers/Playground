using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playground.core.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Authentication;
using AspNet.Security.OpenIdConnect.Server;

namespace IdentitySample.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<PlaygroundUser> m_userManager;
        private readonly SignInManager<PlaygroundUser> m_signInManager;

        public AccountController(
            UserManager<PlaygroundUser> userManager,
            SignInManager<PlaygroundUser> signInManager)
        {
            m_userManager = userManager;
            m_signInManager = signInManager;
        }
        
        //
        // POST: /Account/Login 
        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await m_signInManager.PasswordSignInAsync(userName, password, true, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            return Forbid();
        }

        //
        // POST: /Account/Register
        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register(string userName, string password)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new PlaygroundUser { UserName = userName };
            var result = await m_userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await m_signInManager.PasswordSignInAsync(userName, password, true, false);
            }
            else
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        //
        // POST: /Account/LogOff
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await m_signInManager.SignOutAsync();
            return Ok();
        }
    }
}