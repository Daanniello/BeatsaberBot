using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Security
{
    public class RateLimit
    {
        public int callsBeforeLimit;

        private Dictionary<ulong, int> UserWithRecentCall = new Dictionary<ulong, int>();


        public RateLimit(int callsEachMinute)
        {
            callsBeforeLimit = callsEachMinute;
            ResetRateLimit(60000);
        }

        private async Task ResetRateLimit(int interval)
        {
            while (true)
            {
                await Task.Delay(interval);
                UserWithRecentCall.Clear();
            }
        }

        public int AddCall(ulong discordID)
        {
            UserWithRecentCall.TryGetValue(discordID, out var value);
            if (value == 0)
            {
                UserWithRecentCall.Add(discordID, 1);
                return 0;
            }
            else
            {
                UserWithRecentCall[discordID] += 1;
                return UserWithRecentCall[discordID];
            }
        }

        public bool IsUserRateLimited(ulong discordID)
        {
            UserWithRecentCall.TryGetValue(discordID, out var value);
            if (value == 0) return false;
            else
            {
                if (UserWithRecentCall[discordID] > callsBeforeLimit)
                {
                    return true;
                }
                else return false;
            }
        }
    }
}
