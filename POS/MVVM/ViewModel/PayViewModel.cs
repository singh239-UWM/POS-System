using MySql.Data.MySqlClient;
using POS.Core;
using POS.MVVM.Models;
using POS.Services;
using POS.Store;
using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows;

namespace POS.MVVM.ViewModel
{
    public class PayViewModel : Core.ViewModel
    {
        private ConnStore _connStore;

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
                            ReceiptItemsStore recptItemStore, ConnStore connStore)
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
            _connStore = connStore;

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
                _isNegSel = false;
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
                _isNegSel = false;
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
                // clear amounts
                AmountEntered = "0.00";
                _amtEnteredStr = "";
                _isNegSel = false;
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
                        double amtTendered = _recptAmtDueStore.RecptDueAmount[1] + amountEntered;
                        double change = amtTendered - _recptAmtDueStore.RecptDueAmount[0];

                        #region making sql transaction
                        MySqlTransaction transaction = null;

                        try
                        {
                            // Begin a new transaction
                            transaction = _connStore.CurrentCon.BeginTransaction();

                            long lastInsertedId = -1;
                            
                            //add receipt to sql
                            string sqlAddRecptQuery = "INSERT INTO pos_1.receipt (recpt_emp, recpt_subTotal, recpt_tax, recpt_disc, recpt_total, recpt_due_amt_tend, recpt_due_change, recpt_date_time) " +
                                                      "VALUES (@recpt_emp, @recpt_subTotal, @recpt_tax, @recpt_disc, @recpt_total, @recpt_due_amt_tend, @recpt_due_change, @recpt_date_time);";
                            using (MySqlCommand cmd = new MySqlCommand(sqlAddRecptQuery, _connStore.CurrentCon))
                            {
                                cmd.Parameters.AddWithValue("@recpt_emp", Convert.ToInt16(_connStore.UserID));
                                cmd.Parameters.AddWithValue("@recpt_subTotal", _recptAmountStore.RecptAmount[0]);
                                cmd.Parameters.AddWithValue("@recpt_disc", _recptAmountStore.RecptAmount[1]);
                                cmd.Parameters.AddWithValue("@recpt_tax", _recptAmountStore.RecptAmount[2]);
                                cmd.Parameters.AddWithValue("@recpt_total", _recptAmountStore.RecptAmount[3]);
                                cmd.Parameters.AddWithValue("@recpt_due_amt_tend", amtTendered);
                                cmd.Parameters.AddWithValue("@recpt_due_change", change);
                                cmd.Parameters.AddWithValue("@recpt_date_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // Execute the query and get the last inserted ID
                                Convert.ToInt32(cmd.ExecuteScalarAsync().Result);
                                lastInsertedId = cmd.LastInsertedId;

                                //lastInsertedId = cmd.LastInsertedId;
                            }

                            if(lastInsertedId < 0)
                            {
                                _windowService.ClosePayWindow();
                                return;
                            }

                            //add each item to from receipt to database
                            string sqlAddRecptItems = "INSERT INTO pos_1.product_to_receipt (recpt_id, prod_upc, prod_desc, prod_price, prod_quan, prod_tot_Price) " +
                                           "VALUES (@recpt_id, @prod_upc, @prod_desc, @prod_price, @prod_quan, @prod_tot_Price);";

                            foreach (Item item in _recptItemStore.ReceiptItems)
                            {
                                using (MySqlCommand cmd = new MySqlCommand(sqlAddRecptItems, _connStore.CurrentCon))
                                {
                                    cmd.Parameters.AddWithValue("@recpt_id", lastInsertedId);
                                    cmd.Parameters.AddWithValue("@prod_upc", item.UPC);
                                    cmd.Parameters.AddWithValue("@prod_desc", item.Description);
                                    cmd.Parameters.AddWithValue("@prod_price", item.Price);
                                    cmd.Parameters.AddWithValue("@prod_quan", item.Quantity);
                                    cmd.Parameters.AddWithValue("@prod_tot_Price", item.TotalPrice);

                                    var res = cmd.ExecuteNonQueryAsync().Result;
                                }
                            }

                            //update the inventory in database
                            string sqlUpdateInven = "UPDATE pos_1.products SET prod_inv = prod_inv - @prod_inv WHERE prod_upc = @prod_upc;";

                            foreach (Item item in _recptItemStore.ReceiptItems)
                            {
                                bool parseRes = int.TryParse(item.UPC, out _);
                                if (parseRes)
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(sqlUpdateInven, _connStore.CurrentCon))
                                    {
                                        cmd.Parameters.AddWithValue("@prod_inv", item.Quantity);
                                        cmd.Parameters.AddWithValue("@prod_upc", item.UPC);

                                        var res = cmd.ExecuteNonQueryAsync().Result;
                                    }
                                }

                            }

                            //clear recipt
                            _recptItemStore.ReceiptItems.Clear();
                            for (int i = 0; i < 4; i++)
                            {
                                _recptAmountStore.RecptAmount[i] = 0.00;
                            }
                            _recptAmountStore.IsTaxFree = false;
                            _recptAmountStore.TaxAmtStore = 0.0;

                            //update due amount 
                            _recptAmtDueStore.RecptDueAmount[1] = amtTendered;
                            _recptAmtDueStore.RecptDueAmount[2] = 0.00;
                            _recptAmtDueStore.RecptDueAmount[3] = change;

                            //amounts
                            AmountEntered = "0.00";
                            _amtEnteredStr = "";
                            _isNegSel = false;

                            //compleate transaction
                            transaction.Commit();

                            //close the pay window
                            _windowService.ClosePayWindow();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.InnerException.Message);

                            // If an exception occurred, rollback the transaction to undo the changes
                            if (transaction != null)
                            {
                                transaction.Rollback();
                                Console.WriteLine("Changes reverted successfully.");
                            }

                            _windowService.ClosePayWindow();
                            return;
                        }
                        #endregion

                    }
                    else
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[2] -= amountEntered;
                        //amounts
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";
                        _isNegSel = false;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Cash Amount");
                    AmountEntered = "0.00";
                    _amtEnteredStr = "";
                    _isNegSel = false;
                }
            }
            else //Return Transaction
            {
                if(amountEntered > 0) //cust gave money
                {
                    //attention graber
                    string messageBoxText = "Do you want tender customer amount on RETURN?";
                    string caption = "Return Amount";
                    MessageBoxButton button = MessageBoxButton.YesNoCancel;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result;

                    result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                            break;
                        case MessageBoxResult.Yes:
                            _recptAmtDueStore.RecptDueAmount[2] -= amountEntered;
                            break;
                        case MessageBoxResult.No:
                            break;
                    }

                    

                    //clear entered amts
                    AmountEntered = "0.00";
                    _amtEnteredStr = "";
                    _isNegSel = false;
                }
                else if(amountEntered < 0) //cashier is giving money to cust
                {
                    double amountDue = Math.Round(_recptAmtDueStore.RecptDueAmount[2], 2);
                    if((amountEntered - amountDue) == 0) // Amount due will be paid
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[2] = 0;
                        _recptAmtDueStore.RecptDueAmount[3] = -(_recptAmtDueStore.RecptDueAmount[1]);

                        try
                        {
                            int lastInsertedId = -1;
                            string sqlQuery = "INSERT INTO pos_1.receipt (recpt_emp, recpt_subTotal, recpt_disc, recpt_tax, recpt_total) " +
                                              "VALUES (@recpt_emp, @recpt_subTotal, @recpt_disc, @recpt_tax, @recpt_total);";
                            using (MySqlCommand cmd = new MySqlCommand(sqlQuery, _connStore.CurrentCon))
                            {
                                cmd.Parameters.AddWithValue("@recpt_emp", Convert.ToInt16(_connStore.UserID));
                                cmd.Parameters.AddWithValue("@recpt_subTotal", _recptAmountStore.RecptAmount[0]);
                                cmd.Parameters.AddWithValue("@recpt_disc", _recptAmountStore.RecptAmount[1]);
                                cmd.Parameters.AddWithValue("@recpt_tax", _recptAmountStore.RecptAmount[2]);
                                cmd.Parameters.AddWithValue("@recpt_total", _recptAmountStore.RecptAmount[3]);

                                // Execute the query and get the last inserted ID
                                lastInsertedId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            if (lastInsertedId < 0)
                            {
                                _windowService.ClosePayWindow();
                                return;
                            }

                            try
                            {
                                sqlQuery = "INSERT INTO pos_1.product_to_receipt (recpt_id, prod_upc, prod_desc, prod_price, prod_quan, prod_tot_Price) " +
                                           "VALUES (@recpt_id, @prod_upc, @prod_desc, @prod_price, @prod_quan, @prod_tot_Price);";
                                foreach (Item item in _recptItemStore.ReceiptItems)
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(sqlQuery, _connStore.CurrentCon))
                                    {
                                        cmd.Parameters.AddWithValue("@recpt_id", lastInsertedId);
                                        cmd.Parameters.AddWithValue("@prod_upc", item.UPC);
                                        cmd.Parameters.AddWithValue("@prod_desc", item.Description);
                                        cmd.Parameters.AddWithValue("@prod_price", item.Price);
                                        cmd.Parameters.AddWithValue("@prod_quan", item.Quantity);
                                        cmd.Parameters.AddWithValue("@prod_tot_Price", item.TotalPrice);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                _windowService.ClosePayWindow();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            _windowService.ClosePayWindow();
                            return;
                        }

                        //clear recipt
                        _recptItemStore.ReceiptItems.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            _recptAmountStore.RecptAmount[i] = 0.00;
                        }

                        //amounts
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";
                        _isNegSel = false;

                        //close the pay window
                        _windowService.ClosePayWindow();
                    }
                    else if((amountEntered - amountDue) > 0)
                    {
                        _recptAmtDueStore.RecptDueAmount[1] += amountEntered;
                        _recptAmtDueStore.RecptDueAmount[2] = _recptAmtDueStore.RecptDueAmount[2] - _recptAmtDueStore.RecptDueAmount[1];

                        AmountEntered = "0.00";
                        _amtEnteredStr = "";
                        _isNegSel = false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid Cash Amount");
                        AmountEntered = "0.00";
                        _amtEnteredStr = "";
                        _isNegSel = false;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Cash Amount");
                    AmountEntered = "0.00";
                    _amtEnteredStr = "";
                    _isNegSel = false;
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
                _recptAmountStore.IsTaxFree = false;
                _recptAmountStore.RecptAmount[2] = _recptAmountStore.TaxAmtStore;
                _recptAmountStore.RecptAmount[3] = _recptAmountStore.RecptAmount[0] + _recptAmountStore.RecptAmount[1] + _recptAmountStore.TaxAmtStore;
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
