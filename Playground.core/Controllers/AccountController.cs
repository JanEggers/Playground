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
using AspNet.Security.OpenIdConnect.Primitives;
using System.Diagnostics;
using OpenIddict.Core;
using System.Linq;
using Microsoft.Extensions.Options;

namespace IdentitySample.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<PlaygroundUser> m_userManager;
        private readonly SignInManager<PlaygroundUser> m_signInManager;
        private readonly IOptions<IdentityOptions> m_identityOptions;

        public AccountController(
            UserManager<PlaygroundUser> userManager,
            SignInManager<PlaygroundUser> signInManager,
            IOptions<IdentityOptions> identityOptions)
        {
            m_userManager = userManager;
            m_signInManager = signInManager;
            m_identityOptions = identityOptions;
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

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
            Debug.Assert(request.IsTokenRequest(),
                "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
                "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            if (request.IsPasswordGrantType())
            {
                var user = await m_userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Ensure the user is allowed to sign in.
                if (!await m_signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The specified user is not allowed to sign in."
                    });
                }

                // Reject the token request if two-factor authentication has been enabled by the user.
                if (m_userManager.SupportsUserTwoFactor && await m_userManager.GetTwoFactorEnabledAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The specified user is not allowed to sign in."
                    });
                }

                // Ensure the user is not already locked out.
                if (m_userManager.SupportsUserLockout && await m_userManager.IsLockedOutAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Ensure the password is valid.
                if (!await m_userManager.CheckPasswordAsync(user, request.Password))
                {
                    if (m_userManager.SupportsUserLockout)
                    {
                        await m_userManager.AccessFailedAsync(user);
                    }

                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                if (m_userManager.SupportsUserLockout)
                {
                    await m_userManager.ResetAccessFailedCountAsync(user);
                }

                // Create a new authentication ticket.
                var ticket = await CreateTicketAsync(request, user);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/refresh token.
                var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the authorization code/refresh token.
                // Note: if you want to automatically invalidate the authorization code/refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                var user = await m_userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The token is no longer valid."
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await m_signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Create a new authentication ticket, but reuse the properties stored in the
                // authorization code/refresh token, including the scopes originally granted.
                var ticket = await CreateTicketAsync(request, user, info.Properties);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(
            OpenIdConnectRequest request, PlaygroundUser user,
            AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await m_signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIdConnectServerDefaults.AuthenticationScheme);

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
            }

            ticket.SetResources("resource_server");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == m_identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }
    }
}