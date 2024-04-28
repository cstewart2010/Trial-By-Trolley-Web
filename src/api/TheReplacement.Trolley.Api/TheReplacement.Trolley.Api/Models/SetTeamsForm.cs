using System;
using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Client.Models
{
    internal class SetTeamsForm
    {
        public PlayerSelectionItem[] PlayerSelections { get; set; }
    }

    internal class PlayerSelectionItem
    {
        public Guid PlayerId { get; set; }
        public Team Team { get; set; }
    }
}
