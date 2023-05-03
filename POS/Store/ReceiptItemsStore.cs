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
        private Item _selectedItem;

        public ObservableCollection<Item> ReceiptItems
        {
            get { return _receiptItems; }
            set
            {
                _receiptItems = value;
                OnPropertyChanged(nameof(ReceiptItems));
            }
        }

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ReceiptItemsStore()
        {
            ReceiptItems = new ObservableCollection<Item>();
            SelectedItem = null;
        }
    }
}
