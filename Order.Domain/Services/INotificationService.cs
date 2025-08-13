using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string deviceToken, string title, string body);
    }
}
