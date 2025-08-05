using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        [JsonPropertyName("productname")]
        public string Name { get; set; }
        [JsonPropertyName("category")]
        public string Category { get; set; }
        [JsonPropertyName("company")]
        public string Company { get; set; }
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
       
    }
}
