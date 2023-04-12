
namespace POS.MVVM.Models
{
    public class Item
    {
        public int Index { get; set; }
        public string UPC { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public Item(int index, string upc, string description, double price, int quantity, double totalPrice = 0.00)
        {
            Index = index;
            UPC = upc;
            Description = description;
            Price = price;
            Quantity = quantity;
            TotalPrice = price * quantity;
        }
    }
}
