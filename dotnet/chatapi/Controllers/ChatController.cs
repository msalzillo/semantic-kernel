// Copyright (c) Microsoft. All rights reserved.

using Grafedi.LLM.ChatAPI.Models;
using Grafedi.LLM.ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grafedi.LLM.ChatAPI.Controllers;
[Route("api/chat")]
[ApiController]

public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly ICompletionService _completionService;

    public ChatController(ILogger<ChatController> logger, ICompletionService completionService)
    {
        this._logger = logger;
        this._completionService = completionService;
    }

    [HttpPost("ask", Name = "AskQuestion")]
    public async Task<IActionResult> AskQuestionAsync([FromBody] AskRequest question)
    {
        AskResponse response = await this._completionService.AskQuestion(question);
        return this.Ok(response);
    }
}
