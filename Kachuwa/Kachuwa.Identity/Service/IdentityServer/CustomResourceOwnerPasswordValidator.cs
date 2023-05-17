using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
using Kachuwa.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Kachuwa.Identity.Service
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IAppUserService _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public CustomResourceOwnerPasswordValidator(IAppUserService userRepository,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, context.Password, true);
                if (result.Succeeded)
                {
                    //TODO:: grab other request object and issue claim based on value
                    var value=context.Request.Raw.Get("originrequest");
                    
                    var appuser = await _userRepository.AppUserCrudService.GetAsync(
                        "Where IsActive=@isActive and Email=@email", new {email = context.UserName, isActive = true});
                 
                    if (!string.IsNullOrEmpty(value))
                    {
                        //if (value == "pos")
                        //{
                        //    context.Result = new GrantValidationResult(appuser.IdentityUserId.ToString()
                        //        , OidcConstants.AuthenticationMethods.Password, new List<Claim>()
                        //        {
                        //            new Claim("platform", "Pos")
                        //        });
                        //}
                        //else if (value == "apps")
                        //{
                        //    context.Result = new GrantValidationResult(appuser.IdentityUserId.ToString()
                        //        , OidcConstants.AuthenticationMethods.Password, new List<Claim>()
                        //        {
                        //            new Claim("platform", "apps")
                        //        });
                        //}
                        //else
                        //{
                        //    context.Result = new GrantValidationResult(appuser.IdentityUserId.ToString()
                        //        , OidcConstants.AuthenticationMethods.Password, new List<Claim>()
                        //        {
                        //            new Claim("platform", "Web")
                        //        });
                        //}
                        context.Result = new GrantValidationResult(appuser.IdentityUserId.ToString()
                            , OidcConstants.AuthenticationMethods.Password);

                    }
                    else
                    {
                        context.Result = new GrantValidationResult(appuser.IdentityUserId.ToString()
                            , OidcConstants.AuthenticationMethods.Password);
                    }
                   
                }

                //if (_userRepository.ValidateCredentials(context.UserName, context.Password))
                //{
                //    var user = _userRepository.FindByUsername(context.UserName);
                //    context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password);
                //}
            }

            await Task.FromResult(0);
        }
    }
}