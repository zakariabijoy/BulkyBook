using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BulkyBook.Models
{
    public class OrderDetails
    {
        public int  Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Count { get; set; }
        public double Price { get; set; }

    }
}
