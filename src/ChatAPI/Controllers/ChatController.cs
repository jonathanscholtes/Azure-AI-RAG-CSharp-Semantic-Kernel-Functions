using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers;


  public class ChatRequest
        {
            public string Input { get; set; }
            public string SessionId { get; set; }
        }



[ApiController]
[Route("[controller]")]
public sealed class ChatController(ILogger<ChatController> logger, ChatService chatService) : ControllerBase
{
    [HttpPost(Name = "PostChatRequest")]
    public async Task<string> Post([FromBody] ChatRequest request)
    {   
         string input = request.Input;
        string sessionId = request.SessionId;

        string result = await chatService.GetResponseAsync( input);
        logger.LogInformation("Result: {Result}", result);
        return result;
    }
}