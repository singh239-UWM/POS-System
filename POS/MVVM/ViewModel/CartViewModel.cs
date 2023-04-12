using POS.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using RelayUICommand = Wpf.Ui.Common.RelayCommand;
using POS.Services;
using RelayCommand = POS.Core.RelayCommand;
using POS.Store;
using POS.MVVM.View;
using POS.MVVM.Models;

namespace POS.MVVM.ViewModel
{
    public class CartViewModel : Core.ViewModel
    {
        private IWindowService _windowService;
        private INavigationService _navigation;

        private ObservableCollection<Item> _receipt;
        private ReceiptItemsStore _receipItemStore;
        private Item _selectedItem;
        private string _upcEntered = "";
        private int _quantityEntered = 1;
        private ObservableCollection<double> _recptAmounts;
        private ReceiptAmountStore _recptAmountStore;
        private ReceiptAmountDueStore _recptAmtDueStore;

        public int INDEX = 0;

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
                _receipItemStore.ReceiptItems = _receipt;
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
                return (Item)_selectedItem;
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
                _recptAmountStore.RecptAmount = _recptAmounts;
            }
        }

        public RelayCommand PlusQuanComm { get; set; }
        public RelayCommand MinusQuanComm { get; set; }
        public RelayCommand DeleteItemComm { get; set; }
        public RelayCommand PayComm { get; set; }
        public RelayCommand TaxFreeComm { get; set; }

        public CartViewModel(INavigationService navService, IWindowService windowService, 
                             ReceiptAmountStore recptAmountStore, ReceiptItemsStore receiptItemsStore,
                             ReceiptAmountDueStore recptAmtDueStore)
        {
            Navigation = navService;
            _windowService = windowService;
            _recptAmountStore = recptAmountStore;
            _receipItemStore = receiptItemsStore;
            _recptAmtDueStore = recptAmtDueStore;

            PlusQuanComm = new RelayCommand(o => { PlusQuan(); }, canExecute: o => true);
            MinusQuanComm = new RelayCommand(o => { MinusQuan(); }, canExecute: o => true);
            DeleteItemComm = new RelayCommand(o => { DeleteItem(); }, canExecute: o => true);
            PayComm = new RelayCommand(o => { Pay(); }, canExecute: o => true);
            TaxFreeComm = new RelayCommand(o => { TaxFree(); }, canExecute: o => true);

            Receipt = receiptItemsStore.ReceiptItems;

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

        private void Pay()
        {
            if (RecptAmounts[3] == 0.00) { return; }

            _recptAmountStore.UnChangeRecptAmount = RecptAmounts;
            bool opned = _windowService.OpenPayWindow<PayViewModel>();
            if(opned) 
            {
                _recptAmtDueStore.RecptDueAmount[0] = RecptAmounts[3];
                _recptAmtDueStore.RecptDueAmount[1] = 0.00;
                _recptAmtDueStore.RecptDueAmount[2] = RecptAmounts[3];
                _recptAmtDueStore.RecptDueAmount[3] = 0.00;
                Navigation.ChangeCustFaceView<CustFaceDueViewModel>();
            }
        }

        private void DeleteItem()
        {
            if(Receipt.Count < 0) { return; }

            int index = Receipt.IndexOf(SelectedItem);
            if(index < 0) { return; }

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

        private void TaxFree()
        {
            RecptAmounts[2] = 0.00;
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
                INDEX = 0;
            }
            ICollectionView view;
            //Debug.WriteLine(UPCEntered.ToString());
            QuantityEntered = 1;
            for (int i = 0; i < Receipt.Count; i++)
            {
                if (Receipt.ElementAt<Item>(i).UPC == UPCEntered)
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
            Receipt.Add(new Item(INDEX++, UPCEntered, "Turkey", 1.99, 1));

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
