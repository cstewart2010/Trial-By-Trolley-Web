using System;

namespace TheReplacement.Trolley.Api.Client.Models
{
    internal class RemoveGameForm
    {
        public Guid PlayerId { get; set; }
        public Guid HostId { get; set; }
    }
}
