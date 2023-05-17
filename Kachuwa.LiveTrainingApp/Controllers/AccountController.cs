using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Kachuwa.Configuration;
using Kachuwa.Extensions;
using Kachuwa.Identity;
using Kachuwa.Identity.Extensions;
using Kachuwa.Identity.Models;
using Kachuwa.Identity.Service;
using Kachuwa.Identity.ViewModels;
using Kachuwa.Localization;
using Kachuwa.Log.Insight;
using Kachuwa.Web;
using Kachuwa.Web.Extensions;
using Kachuwa.Web.Services;
using Microsoft.AspNetCore.Http;
using IdentityUser = Kachuwa.Identity.Models.IdentityUser;
using Kachuwa.Log;

namespace KachuwaApp
{
    //[Authorize]
    //[SecurityHeaders]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<Kachuwa.Identity.Models.IdentityUser> _userManager;
        private readonly SignInManager<Kachuwa.Identity.Models.IdentityUser> _signInManager;
        private IEmailSender _emailSender;
        private readonly ILogger _logger;

        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly IEventService _eventService;
        private readonly IAppUserService _appUserService;
        private readonly KachuwaAppConfig _kachuwaConfig;
        private readonly AccountService _account;

        public AccountController(
            UserManager<Kachuwa.Identity.Models.IdentityUser> userManager,
            SignInManager<Kachuwa.Identity.Models.IdentityUser> signInManager,
            IEmailSender emailSender,
            //ILogger<AccountController> logger,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationSchemeProvider schemeProvider
            , IOptionsSnapshot<KachuwaAppConfig> kachuwaConfig, ILocaleResourceProvider localeResourceProvider
            , IAppUserService appUserService,
            ILogger logger
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _interaction = interaction;
            _localeResourceProvider = localeResourceProvider;
            _appUserService = appUserService;
            _kachuwaConfig = kachuwaConfig.Value;
            _account = new AccountService(interaction, httpContextAccessor, schemeProvider, clientStore);
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View();
        }

        [HttpPost] 
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["EventImage"] = "";
            if (User.IsAuthenticated())
            {
                _logger.Log(LogType.Info, () =>"auth");
                return Redirect("/");
            }
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.Log(LogType.Info, () => "user logged in");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.Log(LogType.Info, () => "two fact auth");
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.Log(LogType.Info, () => "user locked out");
                    return RedirectToAction(nameof(Lockout));
                }
                else if (result.IsNotAllowed)
                {
                    _logger.Log(LogType.Info, () => "check email for verification");
                    ModelState.AddModelError(String.Empty, _localeResourceProvider.Get("Please Check Your Email for Verification.</br><a href='/account/ReSendEmailVerificationLink' style='color:blue;'>Click Here to Resend Verification link</a>"));
                }
                else
                {
                    _logger.Log(LogType.Info, () => "invalid");
                    ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Invalid username or password."));
                    return View(model);
                }
            }
            _logger.Log(LogType.Info, () => "outside");
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                //_logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                //_logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                //_logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                //_logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                //_logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                //_logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var newAppUser = model.To<UserViewModel>();
                newAppUser.UserName = model.Email;
                
                newAppUser.UserRoles = new List<UserRolesSelected>
                {
                    new UserRolesSelected()
                    {
                        Name ="User",
                        IsSelected = true,
                        RoleId = 3
                    }
                };
                var status = await _appUserService.SaveNewUserAsync(newAppUser);
                if (!status.HasError)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    //_logger.LogInformation("User created a new account with password.");
                    if (_kachuwaConfig.RequireConfirmedEmail)
                    {

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                        return View("RegisterConfirm");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //_logger.LogInformation("User created a new account with password.");
                        return RedirectToLocal(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, status.Message);
                    // AddErrors(status.Message);

                }


            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await _account.BuildLogoutViewModelAsync(logoutId);

            //if (vm.ShowLogoutPrompt == false)
            //{
            var x = await _account.BuildLoggedOutViewModelAsync(vm.LogoutId);

            await _signInManager.SignOutAsync();
            //_logger.LogInformation("User logged out.");

            // check if we need to trigger sign-out at an upstream identity provider
            if (x.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                // hack: try/catch to handle social providers that throw
                return SignOut(new AuthenticationProperties { RedirectUri = url }, x.ExternalAuthenticationScheme);
            }
            // }

            return Redirect("/");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            var vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);

            await _signInManager.SignOutAsync();
            //_logger.LogInformation("User logged out.");

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                // hack: try/catch to handle social providers that throw
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                //_logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Redirect("/");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                var to = new List<EmailAddress>() {
                    new EmailAddress{
                    Email=model.Email }
            };
                await _emailSender.SendEmailAsync("Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>", to.ToArray());
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {


            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
            //if (User.Identity.GetRoles().Contains(KachuwaRoleNames.Contestant))
            //{
            //    return Redirect("/contestant/dashboard");
            //}
            //if (User.Identity.IsAuthenticated == false)
            //{
            //    return Redirect("/contestant/dashboard");
            //}
            //return Redirect("/admin/dashboard");
        }



        //chech user login
        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> UserLogin([FromBody]LoginViewModel model,string returnUrl = null)
        {
            ViewData["EventImage"] = "";
            //if (User.IsAuthenticated())
            //{
            //    return Redirect("/");
            //    return Json(new { Code = 200, Message = "", Data = "OK" });
            //}
            //ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User logged in.");
                    return Json(new { Code = 200, Message = "success", Data = "OK" });
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("User account locked out.");
                //    return RedirectToAction(nameof(Lockout));
                //}
                else if (result.IsNotAllowed)
                {
                    //_logger.LogWarning("Please Check Your Email for Verification.");
                    ModelState.AddModelError(String.Empty, _localeResourceProvider.Get("Please Check Your Email for Verification.<br/><a href='/account/ReSendEmailVerificationLink' style='color:blue;'>Click Here to Resend Verification link</a>"));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Incorrect username or password."));
                    //return View(model);
                    return Json(new { Code = 500, Message = "Incorrect", Data = false });
                }
            }
            var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
            //_logger.LogInformation(string.Join(',', d));
            return Json(new { Code = 500, Message = string.Join(',', d), Data = false });
            // If we got this far, something failed, redisplay form
            //return view(model);
        }

        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> UserRegister([FromBody]RegisterViewModel model, string returnUrl = null)
        {
            
            if (ModelState.IsValid)
             {
                var newAppUser = model.To<UserViewModel>();
                newAppUser.UserName = model.Email;
                newAppUser.UserRoles = new List<UserRolesSelected>
                {
                    new UserRolesSelected()
                    {
                        Name ="User",
                        IsSelected = true,
                        RoleId = 3,
                        
                    }
                };
                var status = await _appUserService.SaveNewUserAsync(newAppUser);
                if (!status.HasError)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    //_logger.LogInformation("User created a new account with password.");
                    if (_kachuwaConfig.RequireConfirmedEmail)
                    {

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                        return Json(new { Code = 200, Message = "confirmemail", Data = "OK" });
                        //return View("RegisterConfirm");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //_logger.LogInformation("User created a new account with password.");
                        return Json(new { Code = 200, Message = "success", Data = "OK" });
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, status.Message);
                    return Json(new { Code = 500, Message = status.Message, Data = false });
                }


            }
            else
            {
                var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { Code = 500, Message = string.Join(',', d), Data = false });
            }

            // If we got this far, something failed, redisplay form
           
            //return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ReSendEmailVerificationLink()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReSendEmailVerificationLink(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (_kachuwaConfig.RequireConfirmedEmail)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View("RegisterConfirm");
        }

        #endregion
    }
}
