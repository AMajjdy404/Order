﻿namespace Order.API.Dtos
{
    public class SendNotificationDto
    {
        public string DeviceToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

}
