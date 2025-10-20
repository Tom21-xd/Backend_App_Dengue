using Microsoft.AspNetCore.SignalR;

namespace Backend_App_Dengue.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time case updates
    /// </summary>
    public class CaseHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }

        /// <summary>
        /// Broadcast new case to all connected clients
        /// </summary>
        public async Task NotifyNewCase(int caseId, string message)
        {
            await Clients.All.SendAsync("ReceiveNewCase", caseId, message);
        }

        /// <summary>
        /// Broadcast case update to all connected clients
        /// </summary>
        public async Task NotifyCaseUpdate(int caseId, string message)
        {
            await Clients.All.SendAsync("ReceiveCaseUpdate", caseId, message);
        }

        /// <summary>
        /// Broadcast case deletion to all connected clients
        /// </summary>
        public async Task NotifyCaseDeleted(int caseId)
        {
            await Clients.All.SendAsync("ReceiveCaseDeleted", caseId);
        }
    }
}
