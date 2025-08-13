using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Order.Domain.Services;

namespace Order.Application
{
    public class NotificationService: INotificationService
    {
        
     public async Task SendNotificationAsync(string deviceToken, string title, string body)
     {
        var message = new Message()
        {
            Token = deviceToken,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"Successfully sent message: {response}");
     }

}
}
