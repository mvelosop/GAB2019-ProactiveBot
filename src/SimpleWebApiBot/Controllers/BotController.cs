using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace SimpleWebApiBot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IAdapterIntegration _adapter;

        public BotController(
            ILogger<BotController> logger,
            IAdapterIntegration adapter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        [HttpPost("/simple-bot/messages")]
        public async Task<InvokeResponse> Messages([FromBody]Activity activity)
        {
            _logger.LogTrace("----- BotController - Receiving activity: {@Activity}", activity);

            return await _adapter.ProcessActivityAsync(string.Empty, activity, OnTurnAsync, default);
        }

        private async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.Trim();

                _logger.LogInformation("----- Receiving message activity - Text: {Text}", text);

                // Echo back to the user whatever they typed.
                await turnContext.SendActivityAsync($"You typed \"{text}\"");
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}