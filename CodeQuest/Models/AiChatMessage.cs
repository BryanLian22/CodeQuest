// Purpose: Defines the AiChatMessage data shape shared between repositories and Web Forms pages.
using System;

namespace CodeQuest.Models
{
    [Serializable]
    public sealed class AiChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
