using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.PageModels;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace CRMService.Pages.PageHandlers
{
    public class SignInHandler
    {
        private readonly Hasher hash = new();

        
    }
}
