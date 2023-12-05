using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWConsole.Model
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        [Required(ErrorMessage = "A name is required for a product")]
        public string ProductName { get; set; }
        public int? SupplierId { get; set; }
        public int? CategoryId { get; set; }
        [Required(ErrorMessage = "A quantity-per-unit is required for a product, if it is singular, use 1")]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "Sorry, but you cannot have a blank quantity-per-unit")] //Found how at https://stackoverflow.com/a/11404559
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        [Required(ErrorMessage = "You must specify if the product is discontinued or not")]
        public bool Discontinued { get; set; }

        public virtual Category Category { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
