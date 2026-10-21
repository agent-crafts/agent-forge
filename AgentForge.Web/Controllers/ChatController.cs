using System.Diagnostics;
using AgentForge.Application.Chat.Commands;
using AgentForge.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Web.Controllers;

[Route("chat")]
public sealed class ChatController(ChatCommand chatCommand, ILogger<ChatController> logger) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpPost("messages")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand? command, CancellationToken cancellationToken)
    {
        if (command is null || string.IsNullOrWhiteSpace(command.Message))
            return BadRequest(new { error = "Please enter a message." });

        if (command.Message.Length > 12000)
            return BadRequest(new { error = "Message is too long (maximum 12,000 characters)." });

        try
        {
            var result = await chatCommand.HandleAsync(
                command with { Message = command.Message.Trim() }, cancellationToken);
            return Ok(new { response = result.Response });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Local inference service unavailable");
            return StatusCode(502, new { error = "The local AI service is unavailable. Check that your model server is running." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat request failed");
            return StatusCode(500, new { error = "The AI request failed. Please try again." });
        }
    }
}
