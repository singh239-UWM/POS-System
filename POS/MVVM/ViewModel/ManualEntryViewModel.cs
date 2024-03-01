using POS.Core;
using POS.MVVM.Models;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls.Interfaces;

namespace POS.MVVM.ViewModel
{
    public class ManualEntryViewModel : Core.ViewModel
    {
        #region private variables
        private INavigationService _navigation;

        private ReceiptItemsStore _receipItemStore;
        private readonly ReceiptAmountStore _recptAmountStore;

        private string _amountEntered = "0.00";
        private string _amtEnteredStr = "";
        #endregion

        public string AmountEntered
        {
            get { return _amountEntered; }
            set
            {
                _amountEntered = value;
                OnPropertyChanged();
            }
        }

        #region Commands init
        public RelayCommand KeyDollarAmtComm { get; set; }
        public RelayCommand DepartmentRingComm { get; set; }
        #endregion


        #region contro
        public ManualEntryViewModel(ReceiptAmountStore recptAmountStore, ReceiptItemsStore receiptItemsStore,
                                    INavigationService navService)
        {
            #region Comm def
            KeyDollarAmtComm = new RelayCommand(o => { KeyDollerAmt(o); }, canExecute: o => true);
            DepartmentRingComm = new RelayCommand(o => { DepartmentRing(o); }, canExecute: o => true);
            #endregion

            #region contructor variable assign
            _navigation = navService;

            _recptAmountStore = recptAmountStore;
            _receipItemStore = receiptItemsStore;
            #endregion

        }
        #endregion

        public event EventHandler ReceiptSelectedItemChanged;
        public event EventHandler ReceiptItemChanged;

        private void KeyDollerAmt(object o)
        {
            if (o == null) { return; }

            if (o.ToString() == "CL")
            {
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                return;
            }

            _amtEnteredStr += o.ToString();

            string modifiedAmtStr = ModifyAmt(_amtEnteredStr);

            try
            {
                //test if amount can be converted to double
                Double.Parse(modifiedAmtStr.ToString());
            }
            catch
            {
                // clear amounts
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                MessageBox.Show("Invalid Amount Entered");
                return;
            }

            AmountEntered = modifiedAmtStr;
        }

        private string ModifyAmt(string amountStr)
        {
            string modifiedAmtStr = "";
            if (amountStr.Length <= 2)
            {
                int len = amountStr.Length;
                switch (len)
                {
                    case 1:
                        modifiedAmtStr = "0.0" + amountStr;
                        break;
                    case 2:
                        modifiedAmtStr = "0." + amountStr;
                        break;
                }

            }
            else
            {
                modifiedAmtStr = amountStr.Insert(amountStr.Length - 2, ".");
            }

            return modifiedAmtStr;
        }

        private void DepartmentRing(object o)
        {
            //check if recipt is item
            if (_receipItemStore.ReceiptItems.Count <= 0)
            {
                _navigation.ChangeCustFaceView<CustFaceTotalViewModel>();
            }


            string department = o.ToString();
            double amountEnteredDouble;
            try
            {
                amountEnteredDouble = Double.Parse(AmountEntered.ToString());
            }
            catch
            {
                // clear amounts
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                MessageBox.Show("Invalid Amount Entered");
                return;
            }

            //for non tax department
            if (department == "Non Tax" || department == "Chip")
            {
                //find if the item is already in the receipt
                for (int i = 0; i < _receipItemStore.ReceiptItems.Count; i++)
                {
                    if (_receipItemStore.ReceiptItems.ElementAt<Item>(i).Price == amountEnteredDouble &&
                        _receipItemStore.ReceiptItems.ElementAt<Item>(i).UPC == department)
                    {
                        //update already added item in receipt and return
                        UpdateAlreadyAddedItem(i, false);
                        return;
                    }
                }
                //add new item on receipt
                AddNewItem(department, amountEnteredDouble, false);

            }
            else // for taxed department
            {
                //case where the amount entered is 0.00
                if (amountEnteredDouble <= 0)
                {
                    MessageBox.Show("Invalid Amount Entered");
                    return;
                }

                //find if the item is already in the receipt
                for (int i = 0; i < _receipItemStore.ReceiptItems.Count; i++) 
                {
                    if (_receipItemStore.ReceiptItems.ElementAt<Item>(i).Price == amountEnteredDouble &&
                        _receipItemStore.ReceiptItems.ElementAt<Item>(i).UPC == department)
                    {
                        //update already added item in receipt and return
                        UpdateAlreadyAddedItem(i, true);
                        return;
                    }
                }

                //add new item on receipt
                AddNewItem(department, amountEnteredDouble, true);
            }
        }

        public void AddNewItem(string department, double amount, bool isTaxed)
        {
            if (isTaxed) //taxed
            {
                Item item = new Item(department, department, amount, 1, isTaxed);
                //_receipItemStore.SelectedItem = item;
                _receipItemStore.ReceiptItems.Add(item);
                _recptAmountStore.RecptAmount[0] += amount;
                _recptAmountStore.RecptAmount[2] += ((_recptAmountStore.IsTaxFree) ? 0.00 : (amount * 0.055));
                _recptAmountStore.RecptAmount[3] = _recptAmountStore.RecptAmount[0] + _recptAmountStore.RecptAmount[1] + _recptAmountStore.RecptAmount[2];

                _recptAmountStore.TaxAmtStore += (amount * 0.055);
                ReceiptItemChanged?.Invoke(this, EventArgs.Empty);

                _receipItemStore.SelectedItem = item;
                ReceiptSelectedItemChanged?.Invoke(this, EventArgs.Empty);

                AmountEntered = "0.00";
                _amtEnteredStr = "";
                return;
            }
            else //non taxed
            {
                Item item = new Item(department, department, amount, 1, isTaxed);
                //_receipItemStore.SelectedItem = item;
                _receipItemStore.ReceiptItems.Add(item);
                _recptAmountStore.RecptAmount[0] += amount;
                _recptAmountStore.RecptAmount[2] += 0.00;
                _recptAmountStore.RecptAmount[3] = _recptAmountStore.RecptAmount[0] + _recptAmountStore.RecptAmount[1] + _recptAmountStore.RecptAmount[2];

                _recptAmountStore.TaxAmtStore += 0.00;
                ReceiptItemChanged?.Invoke(this, EventArgs.Empty);

                _receipItemStore.SelectedItem = item;
                ReceiptSelectedItemChanged?.Invoke(this, EventArgs.Empty);

                AmountEntered = "0.00";
                _amtEnteredStr = "";
                return;
            }
        }

        public void UpdateAlreadyAddedItem(int index, bool isTaxed)
        {
            if (isTaxed)
            {
                //increment and update receipt item
                _receipItemStore.ReceiptItems.ElementAt(index).Quantity += 1;
                _receipItemStore.ReceiptItems.ElementAt(index).TotalPrice = (_receipItemStore.ReceiptItems.ElementAt(index).Quantity) * (_receipItemStore.ReceiptItems.ElementAt(index).Price);

                double taxPerItem = _receipItemStore.ReceiptItems.ElementAt(index).Price * 0.055;

                //update prices
                _recptAmountStore.RecptAmount[0] += _receipItemStore.ReceiptItems.ElementAt(index).Price; //subtotal
                _recptAmountStore.RecptAmount[2] += ((_recptAmountStore.IsTaxFree) ? 0.00 : taxPerItem); //tax
                _recptAmountStore.RecptAmount[3] = _recptAmountStore.RecptAmount[0] + _recptAmountStore.RecptAmount[1] + _recptAmountStore.RecptAmount[2];

                _recptAmountStore.TaxAmtStore += taxPerItem;

                ReceiptItemChanged?.Invoke(this, EventArgs.Empty);

                _receipItemStore.SelectedItem = _receipItemStore.ReceiptItems.ElementAt(index);
                ReceiptSelectedItemChanged?.Invoke(this, EventArgs.Empty);

                AmountEntered = "0.00";
                _amtEnteredStr = "";
                return;
            }
            else // non taxed
            {
                //increment and update receipt item
                _receipItemStore.ReceiptItems.ElementAt(index).Quantity += 1;
                _receipItemStore.ReceiptItems.ElementAt(index).TotalPrice = (_receipItemStore.ReceiptItems.ElementAt(index).Quantity) * (_receipItemStore.ReceiptItems.ElementAt(index).Price);

                //update prices
                _recptAmountStore.RecptAmount[0] += _receipItemStore.ReceiptItems.ElementAt(index).Price; //subtotal
                _recptAmountStore.RecptAmount[2] += 0.00; //tax
                _recptAmountStore.RecptAmount[3] = _recptAmountStore.RecptAmount[0] + _recptAmountStore.RecptAmount[1] + _recptAmountStore.RecptAmount[2];

                /*ReceiptItemChanged?.Invoke(this, EventArgs.Empty);
                _receipItemStore.SelectedItem = _receipItemStore.ReceiptItems.ElementAt(index);
                ReceiptSelectedItemChanged?.Invoke(this, EventArgs.Empty);*/

                ReceiptItemChanged?.Invoke(this, EventArgs.Empty);

                _receipItemStore.SelectedItem = _receipItemStore.ReceiptItems.ElementAt(index);
                ReceiptSelectedItemChanged?.Invoke(this, EventArgs.Empty);

                AmountEntered = "0.00";
                _amtEnteredStr = "";
                return;
            }
        }
    }
}
