using Cavista.CTRecruita.Commands.Auth;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Cavista.CTRecruita.Web.RequestModels.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Cavista.CTRecruita.Web.Auth
{
    [Route("auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        public AuthController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            
            var response = await _mediator.Send(new LoginCommand
            {
                Email = login.EmailAddress,
                Password = login.Password,
                RememberMe = login.RememberMe
            });
            return PrepareResponse(response);
        }
    }
}
