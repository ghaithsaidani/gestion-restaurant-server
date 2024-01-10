using Microsoft.AspNetCore.SignalR;

namespace Restaurant_Backend.Models.RealTimeCommunication
{
    public class UpdateHub:Hub
    {
        public async Task SendUpdate(string message)
        {
           await Clients.All.SendAsync("sendUpdate",message);
        }
    }
}
