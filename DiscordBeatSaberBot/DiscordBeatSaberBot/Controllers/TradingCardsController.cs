using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Controllers
{
    [ApiController]
    [Route("api/tradingcards")]
    public class TradingCardsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> GetAll()
        {
            return "yeet";
        }
    }
}
