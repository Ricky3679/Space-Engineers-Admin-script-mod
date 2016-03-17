﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using midspace.adminscripts.Messages.Sync;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;
    public class CommandFactionDemote : ChatCommand
    {
        public CommandFactionDemote()
            : base(ChatCommandSecurity.Admin, "fd", new[] { "/fd" })
        {
        }

        public override void Help(ulong steamId, bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/fd <#>", "Demotes the specified <#> player one level within their faction.");
        }

        public override bool Invoke(ulong steamId, long playerId, string messageText)
        {
            var match = Regex.Match(messageText, @"/fd\s{1,}(?<Key>.+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var playerName = match.Groups["Key"].Value;
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players, p => p != null);
                IMyPlayer selectedPlayer = null;

                var findPlayer = players.FirstOrDefault(p => p.DisplayName.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));
                if (findPlayer != null)
                {
                    selectedPlayer = findPlayer;
                }

                int index;
                if (playerName.Substring(0, 1) == "#" && Int32.TryParse(playerName.Substring(1), out index) && index > 0 && index <= CommandPlayerStatus.IdentityCache.Count)
                {
                    var listplayers = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(listplayers, p => p.PlayerID == CommandPlayerStatus.IdentityCache[index - 1].PlayerId);
                    selectedPlayer = listplayers.FirstOrDefault();
                }

                if (selectedPlayer == null)
                    return false;

                var fc = MyAPIGateway.Session.Factions.GetObjectBuilder();
                var factionBuilder = fc.Factions.FirstOrDefault(f => f.Members.Any(m => m.PlayerId == selectedPlayer.PlayerID));

                if (factionBuilder == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("demote", "{0} not in faction.", selectedPlayer.DisplayName);
                    return true;
                }

                var fm = factionBuilder.Members.FirstOrDefault(m => m.PlayerId == selectedPlayer.PlayerID);

                if (fm.IsFounder)
                {
                    MyAPIGateway.Utilities.ShowMessage("demote", "{0} is Founder and cannot be demoted.", selectedPlayer.DisplayName);
                    return true;
                }

                if (fm.IsLeader)
                {
                    MessageSyncFaction.DemotePlayer(factionBuilder.FactionId, selectedPlayer.PlayerID);
                    MyAPIGateway.Utilities.ShowMessage("demote", "{0} from Leader to Member.", selectedPlayer.DisplayName);
                    return true;
                }

                MyAPIGateway.Utilities.ShowMessage("demote", "{0} cannot be demoted further.", selectedPlayer.DisplayName);
                return true;
            }

            return false;
        }
    }
}
