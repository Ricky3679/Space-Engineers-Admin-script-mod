﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using midspace.adminscripts.Messages.Sync;
    using Sandbox.ModAPI;

    public class CommandPlayerSlay : ChatCommand
    {
        public CommandPlayerSlay()
            : base(ChatCommandSecurity.Admin, "slay", new[] { "/slay" })
        {
        }

        public override void Help(ulong steamId, bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/slay <#>", "Kills the specified <#> player. Instant death in Survival mode for any player.");
        }

        public override bool Invoke(ulong steamId, long playerId, string messageText)
        {
            var match = Regex.Match(messageText, @"/slay\s+(?:(?:""(?<name>[^""]|.*?)"")|(?<name>.*))", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var playerName = match.Groups["name"].Value;
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players, p => p != null);
                IMyPlayer selectedPlayer = null;

                var findPlayer = players.FirstOrDefault(p => p.DisplayName.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));
                if (findPlayer != null)
                {
                    selectedPlayer = findPlayer;
                }

                int index;
                if (playerName.Substring(0, 1) == "#" && int.TryParse(playerName.Substring(1), out index) && index > 0 && index <= CommandPlayerStatus.IdentityCache.Count)
                {
                    var listplayers = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(listplayers, p => p.PlayerID == CommandPlayerStatus.IdentityCache[index - 1].PlayerId);
                    selectedPlayer = listplayers.FirstOrDefault();
                }

                if (selectedPlayer == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("Slay", "No player named '{0}' found.", playerName);
                    return true;
                }

                MessageSyncAres.Slay(selectedPlayer.SteamUserId);
                return true;
            }

            return false;
        }
    }
}
