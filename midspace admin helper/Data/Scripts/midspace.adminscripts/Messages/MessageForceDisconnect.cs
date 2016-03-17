﻿namespace midspace.adminscripts.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using midspace.adminscripts.Messages.Communication;
    using ProtoBuf;
    using Sandbox.ModAPI;
    using VRage.Game.ModAPI;

    [ProtoContract]
    public class MessageForceDisconnect : MessageBase
    {
        [ProtoMember(1)]
        public ulong SteamId;

        [ProtoMember(2)]
        public bool Ban = false;

        public override void ProcessClient()
        {
            if (SteamId == MyAPIGateway.Session.Player.SteamUserId)
                CommandForceKick.DropPlayer = true;
        }

        public override void ProcessServer()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p != null && p.SteamUserId == SteamId);
            IMyPlayer player = players.FirstOrDefault();

            if (Ban)
            {
                ChatCommandLogic.Instance.ServerCfg.Config.ForceBannedPlayers.Add(new Player()
                {
                    SteamId = SteamId,
                    PlayerName = player.DisplayName
                });
            }

            ConnectionHelper.SendMessageToPlayer(SteamId, this);
            MessageClientTextMessage.SendMessage(SenderSteamId, "Server", string.Format("{0} player {1}.", Ban ? "Forcebanned" : "Forcekicked", player.DisplayName));
        }
    }
}
