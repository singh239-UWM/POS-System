using POS.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Store
{
    public class SelectedItemForConfigStore
    {
        ItemForConfig _item;

        public ItemForConfig Item
        {
            get 
            {
                return _item; 
            }
            set 
            { 
                _item = value; 
            }
        }
    }
}
