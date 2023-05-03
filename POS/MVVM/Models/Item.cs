
namespace POS.MVVM.Models
{
    public class Item
    {
        public string UPC { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public bool IsTaxable { get; set; }
        public double TotalPrice { get; set; }
        public Item(string upc, string description, double price, int quantity, bool isTaxable)
        {
            UPC = upc;
            Description = description;
            Price = price;
            Quantity = quantity;
            TotalPrice = price * quantity;
            IsTaxable = isTaxable;
        }
    }
}
