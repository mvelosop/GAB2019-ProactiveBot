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
using SimpleWebApiBot.Timer;

namespace SimpleWebApiBot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IAdapterIntegration _adapter;
        private readonly Timers _timers;

        public BotController(
            ILogger<BotController> logger,
            IAdapterIntegration adapter,
            Timers timers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _timers = timers ?? throw new ArgumentNullException(nameof(timers));
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

                if (text.StartsWith("timer", StringComparison.InvariantCultureIgnoreCase))
                {
                    var seconds = Convert.ToInt32(text.Substring(text.IndexOf(" ")));

                    await turnContext.SendActivityAsync($"Starting a timer to go off in {seconds}s");

                    _timers.AddTimer(turnContext.Activity.GetConversationReference(), seconds);
                }
                else if (text.StartsWith("list", StringComparison.InvariantCultureIgnoreCase))
                {
                    var alarms = string.Join("\n", _timers.List.Select(a => $"- #{a.Number} [{a.Seconds}s] - {a.Status} ({a.Elapsed / 1000:n3}s)"));

                    await turnContext.SendActivityAsync($"**TIMERS**\n{alarms}");
                }
                else
                {
                    // Echo back to the user whatever they typed.
                    await turnContext.SendActivityAsync($"You typed \"{text}\"");
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}