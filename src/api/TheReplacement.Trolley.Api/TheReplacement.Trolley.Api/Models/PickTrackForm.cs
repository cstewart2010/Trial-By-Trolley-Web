using System;
using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Client.Models
{
    internal class PickTrackForm
    {
        public Guid HostId { get; set; }
        public Team Team { get; set; }
    }
}
