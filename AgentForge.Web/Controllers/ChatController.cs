using AgentForge.Application.Chat.Commands;
using AgentForge.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

public class ChatController(ChatCommand chatCommand) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAIConfig()
    {
        var res = await chatCommand.HandleAsync(
            new SendMessageCommand("Hello Sir"));

        return Ok(res);
    }
}