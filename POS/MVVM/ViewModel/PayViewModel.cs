using POS.Core;
using POS.Services;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls.Interfaces;

namespace POS.MVVM.ViewModel
{
    public class PayViewModel : Core.ViewModel
    {
        private ReceiptAmountStore _recptAmountStore;
        private ReceiptAmountDueStore _recptAmtDueStore;
        private ReceiptItemsStore _recptItemStore;
        private IWindowService _windowService;
        private INavigationService _navigation;
        private ObservableCollection<double> _recptAmounts;

        private string _amountEntered = "0.00";
        private string _amtEnteredStr = "";
        private bool _isNegSel = false;

        public ObservableCollection<double> RecptAmounts
        {
            get { return _recptAmounts; }
            set
            {
                _recptAmounts = value;
                OnPropertyChanged();
            }
        }

        public INavigationService Navigation
        {
            get { return _navigation; }
            set
            {
                _navigation = value;
                OnPropertyChanged();
            }
        }

        public string AmountEntered
        {
            get { return _amountEntered; }
            set
            {
                _amountEntered = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand CancleCommand { get; set; }
        public RelayCommand CashCommand { get; set; }
        public RelayCommand NegAmtCommand { get; set; }
        public RelayCommand PayDolAmtComm { get; set; }
        public RelayCommand KeyboardDolAmtComm { get; set; }

        #region constor
        public PayViewModel(ReceiptAmountStore recptAmountStore, ReceiptAmountDueStore recptAmtDueStore,
                            IWindowService windowService, INavigationService navService,
                            ReceiptItemsStore recptItemStore)
        {
            CancleCommand = new RelayCommand(o => { Cancle(); }, canExecute: o => true);
            CashCommand = new RelayCommand(o => { PayDolAmt(AmountEntered); }, canExecute: o => true);
            NegAmtCommand = new RelayCommand(o => { NegAmt(); }, canExecute: o => true);
            PayDolAmtComm = new RelayCommand(o => { PayDolAmt(o); }, canExecute: o => true);
            KeyboardDolAmtComm = new RelayCommand(o => { KeyboardDolAmt(o); }, canExecute: o => true);

            _windowService = windowService;
            Navigation = navService;
            _recptAmountStore = recptAmountStore;
            _recptAmtDueStore = recptAmtDueStore;
            _recptItemStore = recptItemStore;


            RecptAmounts = _recptAmountStore.RecptAmount;


        }
        #endregion

        private void KeyboardDolAmt(object o)
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
                Debug.WriteLine("Error: Amount error");
                // clear amounts
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                MessageBox.Show("Invalid Amount Entered");
                return;
            }

            if(_isNegSel)
            {
                AmountEntered = modifiedAmtStr.Insert(0, "-");
            }
            else
            {
                AmountEntered = modifiedAmtStr;
            }
            

        }

        private void PayDolAmt(object o)
        {
            double amountEntered;
            try
            {
                amountEntered = Double.Parse(o.ToString());
            }
            catch
            {
                Debug.WriteLine("Error: Amount error");
                // clear amounts
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                MessageBox.Show("Invalid Amount Entered");
                return;
            }

            if (_recptAmtDueStore.RecptDueAmount[0] > 0) //sale, 
            {
                if (amountEntered > 0)
                {
                    bool isAmtBig = IsAmountBiggerThanTotal(amountEntered);
                    if (isAmtBig)
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[2] = 0.00;
                        _recptAmtDueStore.RecptDueAmount[3] = _recptAmtDueStore.RecptDueAmount[1] - _recptAmtDueStore.RecptDueAmount[0];

                        //clear recipt
                        _recptItemStore.ReceiptItems.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            _recptAmountStore.RecptAmount[i] = 0.00;
                        }

                        //amounts
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";

                        //close the pay window
                        _windowService.ClosePayWindow();

                    }
                    else
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[2] -= amountEntered;
                        //amounts
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Cash Amount");
                }
            }
            else //Return Transaction
            {
                if(amountEntered > 0) //cust gave money
                {
                    //need attention graber
                    _recptAmtDueStore.RecptDueAmount[2] -= amountEntered;
                }
                else if(amountEntered < 0) //cashier is giving money to cust
                {
                    if((amountEntered - _recptAmtDueStore.RecptDueAmount[2]) == 0)
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[3] = _recptAmtDueStore.RecptDueAmount[1] - _recptAmtDueStore.RecptDueAmount[2];

                        //clear recipt
                        _recptItemStore.ReceiptItems.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            _recptAmountStore.RecptAmount[i] = 0.00;
                        }

                        //amounts
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";

                        //close the pay window
                        _windowService.ClosePayWindow();
                    }
                    _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                    _recptAmtDueStore.RecptDueAmount[2] = _recptAmtDueStore.RecptDueAmount[2] - _recptAmtDueStore.RecptDueAmount[1];
                }
                else
                {
                    MessageBox.Show("Invalid Cash Amount");
                }
            }

        }

        private void NegAmt()
        {
            _isNegSel = !(_isNegSel);
            if(_isNegSel)
            {
                AmountEntered = AmountEntered.Insert(0, "-");
            }
            else
            {
                AmountEntered = AmountEntered.Substring(1);
            }
            
        }

        private void Cancle()
        {
            bool closed = _windowService.ClosePayWindow();
            AmountEntered = "0.00";
            _amtEnteredStr = "";
            if (closed)
            {
                Navigation.ChangeCustFaceView<CustFaceTotalViewModel>();
                _recptAmountStore.RecptAmount = _recptAmountStore.UnChangeRecptAmount;
            }
        }

        private bool IsAmountBiggerThanTotal(double amountEntered)
        {
            if (amountEntered >= _recptAmtDueStore.RecptDueAmount[2])
            {
                return true;
            }
            else
            {
                return false;
            }
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
    }
}
