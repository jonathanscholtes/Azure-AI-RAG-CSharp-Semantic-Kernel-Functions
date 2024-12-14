using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ChatController(ILogger<ChatController> logger, ChatService chatService) : ControllerBase
{
    [HttpPost(Name = "PostChatRequest")]
    public async Task<string> Post(string question)
    {
        string result = await chatService.GetResponseAsync( question);
        logger.LogInformation("Result: {Result}", result);
        return result;
    }
}