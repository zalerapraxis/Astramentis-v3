using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Astramentis.Models
{
    public class ReactionAddedEventMessage
    {
        public IUserMessage Message { get; set; }
        public SocketReaction Reaction { get; set; }
        public SocketGuild Server { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
