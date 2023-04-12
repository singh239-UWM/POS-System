using POS.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Store
{
    public class ReceiptItemsStore : Core.ObservableObject
    {
        private ObservableCollection<Item> _receiptItems;

        public ObservableCollection<Item> ReceiptItems
        {
            get { return _receiptItems; }
            set
            {
                _receiptItems = value;
                OnPropertyChanged(nameof(ReceiptItems));
            }
        }

        public ReceiptItemsStore()
        {
            ReceiptItems = new ObservableCollection<Item>();
        }
    }
}
