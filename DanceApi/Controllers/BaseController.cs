using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;


    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string? GetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
