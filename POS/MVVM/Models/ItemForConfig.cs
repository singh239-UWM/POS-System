using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MVVM.Models
{
    public class ItemForConfig
    {
        public string UPC { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public string SecDescription { get; set; }
        public double Cost { get; set; }
        public double Retail { get; set; }
        public double Inventory { get; set; }
        public bool IsTaxable { get; set; }

        public ItemForConfig(string upc, string department, string desc, string secDesc, double cost, double retail, double inventory, bool isTaxable)
        {
            UPC = upc;
            Department = department;
            Description = desc;
            SecDescription = secDesc;
            Cost = cost;
            Retail = retail;
            Inventory = inventory;
            IsTaxable = isTaxable;
        }
    }
}
