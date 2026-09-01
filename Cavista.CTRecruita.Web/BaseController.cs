using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cavista.CTRecruita.Web
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IMediator _mediator;
        public BaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected long CurrentUserId => GetCurrentUserId();
        protected string CurrentUserName => GetUserName();
        protected string CurrentUserRole => GetCurrentRole();
        protected List<string> CurrentUserRoles => GetCurrentRoles();
        protected long CurrentEmployeeId => GetEmployeeId();
        protected string CurrentUserEmail => GetCurrentUserEmail();
        protected string CurrentHost => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        protected string CurrentIpAddress => HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        private string GetCurrentRole()
        {
            var role = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new UnauthorizedAccessException($"User ID not found");
            }

            return role;
        }
        private List<string> GetCurrentRoles()
        {
            var roles = User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (roles == null || !roles.Any())
            {
                throw new UnauthorizedAccessException("User roles not found");
            }

            return roles;
        }
        private string GetUserName()
        {
            var name = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new UnauthorizedAccessException($"User not found");
            }

            return name;
        }

        private string GetCurrentUserEmail()
        {
            var email = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedAccessException($"User name not found");
            }
            return email;
        }

        protected long EmployerId => GetEmployerId();

        private long GetEmployerId()
        {
            var employerId = User?.Claims.FirstOrDefault(c => c.Type == "EmployerId")?.Value;
            if (string.IsNullOrWhiteSpace(employerId))
            {
                throw new UnauthorizedAccessException($"Employer ID not found");
            }
            if (long.TryParse(employerId, out var id)) { return id; }
            throw new UnauthorizedAccessException("Invalid employerid format in token");
        }

        private long GetEmployeeId()
        {
            var employerId = User?.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
            if (string.IsNullOrWhiteSpace(employerId))
            {
                throw new UnauthorizedAccessException($"Employee ID not found");
            }
            if (long.TryParse(employerId, out var id)) { return id; }
            throw new UnauthorizedAccessException("Invalid employeeid format in token");
        }


        private long GetCurrentUserId()
        {
            var userId = User?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException($"User ID not found");
            }
            if (long.TryParse(userId, out var UserId)) { return UserId; }
            throw new UnauthorizedAccessException("Invalid userid format in token");
        }

        //private bool HasAdminRole()
        //{
        //    var roles = User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        //    return roles != null && roles.Contains(AppConstants.ADMIN_ROLE);
        //}

        //private bool HasActingManagerRole()
        //{
        //    var roles = User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        //    return roles != null && roles.Contains(AppConstants.ACTING_MANAGER_ROLE);
        //}
        protected IActionResult PrepareResponse(ApiResponse response)
        {
            var obj = StatusCode(response.StatusCode, new ApiResponse(response.IsError, response.StatusCode, response.Message, response.Data));
            return obj;
        }
        //protected IActionResult PrepareResponseForValidationFailed(ValidationResult requestValidator)
        //    => PrepareResponse(new ApiResponse(true, StatusCodes.Status400BadRequest, string.Join(",", requestValidator.Errors), requestValidator.Errors));
    }
}
