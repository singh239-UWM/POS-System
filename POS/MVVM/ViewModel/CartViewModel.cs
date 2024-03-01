using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.CRUD;
using POS.Core;
using POS.MVVM.Models;
using POS.Services;
using POS.Store;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RelayCommand = POS.Core.RelayCommand;
using RelayUICommand = Wpf.Ui.Common.RelayCommand;

namespace POS.MVVM.ViewModel
{
    public class CartViewModel : Core.ViewModel
    {
        #region private variables
        private ManualEntryViewModel _manualEntryVM;

        private IWindowService _windowService;
        private INavigationService _navigation;

        private ObservableCollection<Item> _receipt;
        private ReceiptItemsStore _receipItemStore;
        private string _upcEntered = "";
        private int _quantityEntered = 1;
        private bool _isTaxFree = false;
        private ObservableCollection<double> _recptAmounts;
        private readonly ReceiptAmountStore _recptAmountStore;
        private readonly ReceiptAmountDueStore _recptAmtDueStore;
        private ConnStore _connStore;
        #endregion

        public INavigationService Navigation
        {
            get { return _navigation; }
            set
            {
                _navigation = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Item> Receipt
        {
            get
            {
                return _receipt;
            }
            set
            {
                _receipt = value;
                //_receipItemStore.ReceiptItems = _receipt;
                //OnPropertyChanged();
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

        public Item SelectedItem
        {
            get
            {
                return (Item)_receipItemStore.SelectedItem;
            }
            set
            {
                _receipItemStore.SelectedItem = value;
                //Debug.WriteLine(_selectedItem.UPC);
                QuantityEntered = (_receipItemStore.SelectedItem == null) ? 1 : _receipItemStore.SelectedItem.Quantity;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public bool IsTaxFree
        {
            get { return (bool)_isTaxFree; }
            set
            {
                _isTaxFree = value;
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
                _recptAmountStore.RecptAmount = _recptAmounts;
            }
        }

        public RelayCommand PlusQuanComm { get; set; }
        public RelayCommand MinusQuanComm { get; set; }
        public RelayCommand DeleteItemComm { get; set; }
        public RelayCommand PayComm { get; set; }
        public RelayCommand TaxFreeComm { get; set; }

        public CartViewModel(INavigationService navService, IWindowService windowService, ConnStore conn,
                             ReceiptAmountStore recptAmountStore, ReceiptItemsStore receiptItemsStore,
                             ReceiptAmountDueStore recptAmtDueStore, ManualEntryViewModel manualEntryVM)
        {
            _manualEntryVM = manualEntryVM;

            Navigation = navService;
            _windowService = windowService;
            _connStore = conn;
            _recptAmountStore = recptAmountStore;
            _receipItemStore = receiptItemsStore;
            _recptAmtDueStore = recptAmtDueStore;

            PlusQuanComm = new RelayCommand(o => { PlusQuan(); }, canExecute: o => true);
            MinusQuanComm = new RelayCommand(o => { MinusQuan(); }, canExecute: o => true);
            DeleteItemComm = new RelayCommand(o => { DeleteItem(); }, canExecute: o => true);
            PayComm = new RelayCommand(o => { Pay(); }, canExecute: o => true);
            TaxFreeComm = new RelayCommand(o => { TaxFree(); }, canExecute: o => true);

            Receipt = receiptItemsStore.ReceiptItems;
            SelectedItem = receiptItemsStore.SelectedItem;

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


            _manualEntryVM.ReceiptSelectedItemChanged += ManualEntryVM_ReceiptSelectedItemChanged;
            _manualEntryVM.ReceiptItemChanged += ManualEntryVM_ReceiptItemChanged;
        }

        private void ManualEntryVM_ReceiptItemChanged(object sender, EventArgs e)
        {
            Receipt = _receipItemStore.ReceiptItems;
            ICollectionView view = CollectionViewSource.GetDefaultView(Receipt);
            view.Refresh();
            SelectedItem = _receipItemStore.SelectedItem;
        }

        private void ManualEntryVM_ReceiptSelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = _receipItemStore.SelectedItem;
        }

        private void Pay()
        {
            if (RecptAmounts[3] == 0.00) { return; }

            _recptAmountStore.UnChangeRecptAmount = RecptAmounts;
            bool opned = _windowService.OpenPayWindow<PayViewModel>();
            if (opned)
            {
                _recptAmtDueStore.RecptDueAmount[0] = RecptAmounts[3];
                _recptAmtDueStore.RecptDueAmount[1] = 0.00;
                _recptAmtDueStore.RecptDueAmount[2] = RecptAmounts[3];
                _recptAmtDueStore.RecptDueAmount[3] = 0.00;

                UPCEntered = "";
                IsTaxFree = false;

                Navigation.ChangeCustFaceView<CustFaceDueViewModel>();
            }
        }

        private void DeleteItem()
        {
            if (Receipt.Count < 0) { return; }

            int index = Receipt.IndexOf(SelectedItem);
            if (index < 0) { return; }

            int itemQuan = Receipt.ElementAt(index).Quantity;
            double taxAmount = (Receipt.ElementAt(index).IsTaxable) ? (itemQuan * Receipt.ElementAt(index).Price) * 0.055 : 0.0;

            RecptAmounts[0] -= (itemQuan * Receipt.ElementAt(index).Price);
            RecptAmounts[2] -= (IsTaxFree) ? 0.0 : (taxAmount);
            //RecptAmounts[3] -= (IsTaxFree) ? (itemQuan * Receipt.ElementAt(index).Price) : (itemQuan * (Receipt.ElementAt(index).Price + (Math.Abs(taxAmount))));
            RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2];

            _recptAmountStore.TaxAmtStore -= taxAmount;

            Receipt.RemoveAt(index);

            if (Receipt.Count > 0)
            {
                SelectedItem = Receipt.ElementAt(Receipt.Count-1);
            }
            else
            {
                SelectedItem = null;
                IsTaxFree = false;
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

                //Spectial case when quan goes from 1 to 0, mean jump of two prices
                int factor = 1;
                if (QuantityEntered == 0)
                {
                    QuantityEntered = -1;
                    factor = 2;
                }
                int itemIndex = Receipt.IndexOf(SelectedItem);
                double itemPrice = Receipt.ElementAt(itemIndex).Price;
                double taxAmt = Receipt.ElementAt(itemIndex).IsTaxable ? (factor * (itemPrice * 0.055)) : 0.00;
                Receipt.ElementAt(itemIndex).Quantity = QuantityEntered;
                Receipt.ElementAt(itemIndex).TotalPrice -= (factor * itemPrice);

                RecptAmounts[0] -= (factor * itemPrice);
                RecptAmounts[2] -= (IsTaxFree) ? 0.00 : taxAmt;
                //RecptAmounts[3] -= (IsTaxFree) ? ((factor) * (itemPrice)) : (factor) * (itemPrice + (itemPrice * 0.055));
                RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2];


                _recptAmountStore.TaxAmtStore -= taxAmt;

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

                //Spectial case when quan goes from -1 to 0
                int factor = 1;
                if (QuantityEntered == 0)
                {
                    QuantityEntered = 1;
                    factor = 2;
                }

                int itemIndex = Receipt.IndexOf(SelectedItem);

                double itemPrice = Receipt.ElementAt(itemIndex).Price;

                double taxAmt = Receipt.ElementAt(itemIndex).IsTaxable ? (factor * (itemPrice * 0.055)) : 0.00;
                Receipt.ElementAt(itemIndex).Quantity = QuantityEntered;
                Receipt.ElementAt(itemIndex).TotalPrice += (factor * itemPrice);

                RecptAmounts[0] += (factor * itemPrice);
                RecptAmounts[2] += ( (IsTaxFree) ? 0.00 : taxAmt);
                //RecptAmounts[3] = (IsTaxFree) ? ((factor) * (itemPrice)) : ((factor) * (itemPrice + (itemPrice * 0.055)));
                RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2];

                _recptAmountStore.TaxAmtStore += taxAmt;

                ICollectionView view;
                view = CollectionViewSource.GetDefaultView(Receipt);
                view.Refresh();
                view = CollectionViewSource.GetDefaultView(RecptAmounts);
                view.Refresh();
            }
        }

        private void TaxFree()
        {
            _recptAmountStore.IsTaxFree = IsTaxFree;
            if (IsTaxFree)
            {
                RecptAmounts[2] = 0.00;
                RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1];
            }
            else
            {
                RecptAmounts[2] = _recptAmountStore.TaxAmtStore;
                RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2];
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
            if (Receipt.Count <= 0)
            {
                Navigation.ChangeCustFaceView<CustFaceTotalViewModel>();
            }
            ICollectionView view;

            QuantityEntered = 1;

            //check if this UPC exist in database
            int UPCresult = 0; //this will keep the resulut amount
            string sql = "SELECT COUNT(*) FROM pos_1.products WHERE prod_upc = @prod_upc";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connStore.CurrentCon))
            {
                // Set parameter value
                cmd.Parameters.AddWithValue("@prod_upc", UPCEntered);

                // Execute the query and get the result
                UPCresult = Convert.ToInt32(cmd.ExecuteScalarAsync().Result);
            }

            if(UPCresult <= 0)
            {
                MessageBox.Show("This Product does not exist in POS.");
                UPCEntered = "";
                return;
            }


            for (int i = 0; i < Receipt.Count; i++) //find if the item is already entered
            {
                if (Receipt.ElementAt<Item>(i).UPC == UPCEntered)
                {
                    Receipt.ElementAt(i).Quantity += 1;
                    Receipt.ElementAt(i).TotalPrice = (Receipt.ElementAt(i).Quantity) * (Receipt.ElementAt(i).Price);

                    double taxPerItem = (Receipt.ElementAt(i).IsTaxable) ? Receipt.ElementAt(i).Price * 0.055 : 0.0;

                    RecptAmounts[0] += Receipt.ElementAt(i).Price; //subtotal
                    RecptAmounts[2] += ( (IsTaxFree) ? 0.00 : taxPerItem ); //tax
                    //RecptAmounts[3] += (IsTaxFree) ? (Receipt.ElementAt(i).Price) : (Receipt.ElementAt(i).Price + taxPerItem); //total
                    RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2]; //total = subtotal + discount + tax

                    _recptAmountStore.TaxAmtStore += taxPerItem;

                    view = CollectionViewSource.GetDefaultView(Receipt);
                    view.Refresh();
                    view = CollectionViewSource.GetDefaultView(RecptAmounts);
                    view.Refresh();

                    SelectedItem = Receipt.ElementAt(i);

                    UPCEntered = "";
                    return;
                }
            }
            //adding new item into receipt


            string itemQueryUPC;
            string itemQueryDesc;
            double itemQueryPrice;
            bool itemQueryIsTaxable;

            sql = "SELECT prod_upc, prod_desc, prod_retail, prod_istaxable FROM pos_1.products WHERE prod_upc = @prod_upc";
            using (MySqlCommand cmd = new MySqlCommand(sql, _connStore.CurrentCon))
            {
                // Set parameter value
                cmd.Parameters.AddWithValue("@prod_upc", UPCEntered);
                cmd.Parameters.AddWithValue("@prod_desc", UPCEntered);
                cmd.Parameters.AddWithValue("@prod_retail", UPCEntered);
                cmd.Parameters.AddWithValue("@prod_istaxable", UPCEntered);

                // Execute the query and get the result
                using (var rdr = cmd.ExecuteReaderAsync().Result)
                {
                    rdr.Read();
                    itemQueryUPC = rdr[0].ToString();
                    itemQueryDesc = rdr[1].ToString();
                    itemQueryPrice = Convert.ToDouble(rdr[2].ToString());
                    itemQueryIsTaxable = (rdr[3].ToString() == "1") ? true : false;
                }
            }

            Item item = new Item(itemQueryUPC, itemQueryDesc, itemQueryPrice, 1, itemQueryIsTaxable);
            Receipt.Add(item);
            double itemTax = (item.IsTaxable) ? item.Price * 0.055 : 0.0;
            RecptAmounts[0] += item.Price;
            RecptAmounts[2] += (IsTaxFree) ? 0.00 : itemTax;
            RecptAmounts[3] = RecptAmounts[0] + RecptAmounts[1] + RecptAmounts[2]; //total = subtotal + discount + tax


            _recptAmountStore.TaxAmtStore += itemTax;

            view = CollectionViewSource.GetDefaultView(RecptAmounts);
            view.Refresh();

            SelectedItem = Receipt.ElementAt(Receipt.Count-1);
            UPCEntered = "";
        }
        #endregion
    }
}
