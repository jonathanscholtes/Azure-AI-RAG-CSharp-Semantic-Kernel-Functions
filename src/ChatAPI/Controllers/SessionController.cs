using Microsoft.AspNetCore.Mvc;
using System;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("session")] // Prefix for the route
    public class SessionController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult GetSession()
        {
            // Generate a unique session ID using Guid
            var sessionId = Guid.NewGuid().ToString("N"); // Hexadecimal representation without dashes

            return Ok(new { session_id = sessionId });
        }
    }
}
