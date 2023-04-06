using POS.Core;
using Wpf.Ui.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using RelayUICommand = Wpf.Ui.Common.RelayCommand;
using POS.Services;
using RelayCommand = POS.Core.RelayCommand;
using Wpf.Ui.Controls.Interfaces;
using System.Windows.Forms;

namespace POS.MVVM.ViewModel
{
    public class CartViewModel : Core.ViewModel
    {
        #region ReceiptItem class
        public class ReceiptItem
        {
            public int Index { get; set; }
            public string UPC { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
            public double TotalPrice { get; set; }
            public ReceiptItem(int index, string upc, string description, double price, int quantity, double totalPrice = 0.00)
            {
                Index = index;
                UPC = upc;
                Description = description;
                Price = price;
                Quantity = quantity;
                TotalPrice = price * quantity;
            }
        }
        #endregion

        private ObservableCollection<ReceiptItem> _receipt;
        private ReceiptItem _selectedItem;
        private string _upcEntered = "";
        private int _quantityEntered = 1;
        private ObservableCollection<double> _recptAmounts;

        public int INDEX = 0;

        public ObservableCollection<ReceiptItem> Receipt
        {
            get
            {
                return _receipt;
            }
            set
            {
                _receipt = value;
                OnPropertyChanged();
            }
        }

        public string UPCEntered
        {
            get { return _upcEntered; }
            set
            {
                _upcEntered = value;
                OnPropertyChanged();
            }
        }

        public int QuantityEntered
        {
            get { return _quantityEntered; }
            set
            {
                _quantityEntered = value; 
                OnPropertyChanged();
            }
        }

        public ReceiptItem SelectedItem
        {
            get
            {
                return (ReceiptItem)_selectedItem;
            }
            set
            {
                _selectedItem = value;
                //Debug.WriteLine(_selectedItem.UPC);
                QuantityEntered = (_selectedItem == null) ? 1 : _selectedItem.Quantity;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<double> RecptAmounts
        {
            get
            {
                return _recptAmounts;
            }
            set
            {
                _recptAmounts = value;
            }
        }

        public RelayCommand PlusQuanComm { get; set; }
        public RelayCommand MinusQuanComm { get; set; }
        public RelayCommand DeleteItemComm { get; set; }

        public CartViewModel(INavigationService navService)
        {

            PlusQuanComm = new RelayCommand(o => { PlusQuan(); }, canExecute: o => true);
            MinusQuanComm = new RelayCommand(o => { MinusQuan(); }, canExecute: o => true);
            DeleteItemComm = new RelayCommand(o => { DeleteItem(); }, canExecute: o => true);

            Receipt = new ObservableCollection<ReceiptItem>();

            //RecptAmounts[0] -> Subtotal
            //RecptAmounts[1] -> discount
            //RecptAmounts[2] -> tax
            //RecptAmounts[3] -> Total
            RecptAmounts = new ObservableCollection<double>
            {
                0.00,
                0.00,
                0.00,
                0.00
            };
        }

        private void DeleteItem()
        {
            if(Receipt.Count < 0) { return; }

            int index = Receipt.IndexOf(SelectedItem);

            int itemQuan = Receipt.ElementAt(index).Quantity;

            RecptAmounts[0] -= (itemQuan * Receipt.ElementAt(index).Price);
            RecptAmounts[2] -= (itemQuan * Receipt.ElementAt(index).Price * 0.055);
            RecptAmounts[3] -= (itemQuan * (Receipt.ElementAt(index).Price + (Receipt.ElementAt(index).Price * 0.055)));


            Receipt.RemoveAt(index);
            INDEX -= 1;

            if (Receipt.Count > 0)
            {
                SelectedItem = Receipt.ElementAt(INDEX - 1);
            }

            ICollectionView view;
            view = CollectionViewSource.GetDefaultView(Receipt);
            view.Refresh();
            view = CollectionViewSource.GetDefaultView(RecptAmounts);
            view.Refresh();
        }

        private void MinusQuan()
        {
            if (Receipt.Count > 0)
            {
                QuantityEntered -= 1;
                Receipt.ElementAt(SelectedItem.Index).Quantity -= 1;
                Receipt.ElementAt(SelectedItem.Index).TotalPrice -= Receipt.ElementAt(SelectedItem.Index).Price;

                RecptAmounts[0] -= Receipt.ElementAt(SelectedItem.Index).Price;
                RecptAmounts[2] -= Receipt.ElementAt(SelectedItem.Index).Price * 0.055;
                RecptAmounts[3] -= (Receipt.ElementAt(SelectedItem.Index).Price + (Receipt.ElementAt(SelectedItem.Index).Price * 0.055));

                ICollectionView view;
                view = CollectionViewSource.GetDefaultView(Receipt);
                view.Refresh();
                view = CollectionViewSource.GetDefaultView(RecptAmounts);
                view.Refresh();
            }
        }

        private void PlusQuan()
        {
            if (Receipt.Count > 0)
            {
                QuantityEntered += 1;
                Receipt.ElementAt(SelectedItem.Index).Quantity += 1;
                Receipt.ElementAt(SelectedItem.Index).TotalPrice += Receipt.ElementAt(SelectedItem.Index).Price;

                RecptAmounts[0] += Receipt.ElementAt(SelectedItem.Index).Price;
                RecptAmounts[2] += Receipt.ElementAt(SelectedItem.Index).Price * 0.055;
                RecptAmounts[3] += (Receipt.ElementAt(SelectedItem.Index).Price + (Receipt.ElementAt(SelectedItem.Index).Price * 0.055));

                ICollectionView view;
                view = CollectionViewSource.GetDefaultView(Receipt);
                view.Refresh();
                view = CollectionViewSource.GetDefaultView(RecptAmounts);
                view.Refresh();
            }
        }

        #region UPC Textbox enter command
        private RelayUICommand _UPCEnterCommand;
        public ICommand UPCEnterCommand
        {
            get
            {
                if (_UPCEnterCommand == null)
                {
                    _UPCEnterCommand = new RelayUICommand(UPCEnter);
                }

                return _UPCEnterCommand;
            }
        }

        private void UPCEnter()
        {
            if (UPCEntered == "") return;
            ICollectionView view;
            //Debug.WriteLine(UPCEntered.ToString());
            QuantityEntered = 1;
            for (int i = 0; i < Receipt.Count; i++)
            {
                if (Receipt.ElementAt<ReceiptItem>(i).UPC == UPCEntered)
                {
                    Receipt.ElementAt(i).Quantity += 1;
                    Receipt.ElementAt(i).TotalPrice = (Receipt.ElementAt(i).Quantity) * (Receipt.ElementAt(i).Price);

                    RecptAmounts[0] += Receipt.ElementAt(i).Price;
                    RecptAmounts[2] += Receipt.ElementAt(i).Price * 0.055;
                    RecptAmounts[3] += (Receipt.ElementAt(i).Price + (Receipt.ElementAt(i).Price * 0.055));

                    view = CollectionViewSource.GetDefaultView(Receipt);
                    view.Refresh();
                    view = CollectionViewSource.GetDefaultView(RecptAmounts);
                    view.Refresh();

                    SelectedItem = Receipt.ElementAt(i);

                    UPCEntered = "";
                    return;
                }
            }
            Receipt.Add(new ReceiptItem(INDEX++, UPCEntered, "Turkey", 1.99, 1));

            RecptAmounts[0] += 1.99;
            RecptAmounts[2] += 1.99 * 0.055;
            RecptAmounts[3] += (1.99 + (1.99 * 0.055));

            view = CollectionViewSource.GetDefaultView(RecptAmounts);
            view.Refresh();

            SelectedItem = Receipt.ElementAt(INDEX - 1);
            UPCEntered = "";

        }
        #endregion
    }
}
