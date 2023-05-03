using DynamicData;
using MySql.Data.MySqlClient;
using POS.Core;
using POS.MVVM.Models;
using POS.Services;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using RelayCommand = POS.Core.RelayCommand;
using RelayUICommand = Wpf.Ui.Common.RelayCommand;

namespace POS.MVVM.ViewModel
{
    public class ProductConfigViewModel : Core.ViewModel
    {
        private ConnStore _connStore;
        private IWindowService _windowService;
        private SelectedItemForConfigStore _selItemStore;

        private List<string> _departments;
        private string _upcEntered = "";
        private string _prodUPC = null;
        private string _prodDept = null;
        private string _prodDesc = null;
        private string _prodSecDesc = null;
        private string _prodCost = null;
        private string _prodRetail = null;
        private string _prodInv = null;
        private bool _prodIsTaxable = true;
        private bool _isUpcEna = false;
        private bool _isInvEna = false;
        private bool _isDelButtonEna = true;
        private bool _isAddButtonEna = true;
        private bool _isDupButtonEna = true;
        private bool _isPOButtonEna = true;
        private bool _isLookButtonEna = true;
        //private ItemForConfig _itemForConfig;

        public List<string> Departments
        {
            get
            {
                List<string> list = new List<string>();
                string sql = "SELECT dept_desc FROM pos_1.departments;";
                using(MySqlCommand cmd = new MySqlCommand(sql, _connStore.CurrentCon))
                {
                    using (var rdr = cmd.ExecuteReaderAsync().Result)
                    {
                        while (rdr.Read())
                        {
                            list.Add((string)rdr[0]);
                        }
                        rdr.Close();
                        cmd.Dispose();
                        _departments = list;
                        return _departments;
                    }
                }
            }

            set
            {
                _departments = value;
            }
        }
        public ItemForConfig ConfigItem
        {
            get
            {
                return _selItemStore.Item;
            }
            set
            {
                _selItemStore.Item = value;
                OnPropertyChanged();
            }
        }
        public string ProdUPC
        {
            get
            {
                return _prodUPC;
            }
            set
            {
                _prodUPC = value;
                OnPropertyChanged();
            }
        }
        public string ProdDept
        {
            get
            {
                return _prodDept;
            }
            set
            {
                _prodDept = value;
                OnPropertyChanged();
            }
        }
        public string ProdDesc
        {
            get
            {
                return _prodDesc;
            }
            set
            {
                _prodDesc = value;
                OnPropertyChanged();
            }
        }
        public string ProdSecDesc
        {
            get
            {
                return _prodSecDesc;
            }
            set
            {
                _prodSecDesc = value;
                OnPropertyChanged();
            }
        }
        public string ProdCost
        {
            get
            {
                return _prodCost;
            }
            set
            {
                _prodCost = value;
                OnPropertyChanged();
            }
        }
        public string ProdRetail
        {
            get
            {
                return _prodRetail;
            }
            set
            {
                _prodRetail = value;
                OnPropertyChanged();
            }
        }
        public string ProdInv
        {
            get
            {
                return _prodInv;
            }
            set
            {
                _prodInv = value;
                OnPropertyChanged();
            }
        }
        public bool ProdIsTaxable
        {
            get
            {
                return _prodIsTaxable;
            }
            set
            {
                _prodIsTaxable = value;
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
        public bool IsUPCEna
        {
            get
            {
                return _isUpcEna;
            }
            set
            {
                _isUpcEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsInvEna
        {
            get
            {
                return _isInvEna;
            }
            set
            {
                _isInvEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsDelButtonEna
        {
            get
            {
                return _isDelButtonEna;
            }
            set
            {
                _isDelButtonEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsAddButtonEna
        {
            get
            {
                return _isAddButtonEna;
            }
            set
            {
                _isAddButtonEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsDupButtonEna
        {
            get
            {
                return _isDupButtonEna;
            }
            set
            {
                _isDupButtonEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsPOButtonEna
        {
            get
            {
                return _isPOButtonEna;
            }
            set
            {
                _isPOButtonEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsLookButtonEna
        {
            get
            {
                return _isLookButtonEna;
            }
            set
            {
                _isLookButtonEna = value;
                OnPropertyChanged();
            }
        }

        #region command definitions
        public RelayCommand CloseProdConfigComm { get; set; }
        public RelayCommand AddItemComm { get; set; }
        public RelayCommand SaveItemComm { get; set; }
        #endregion

        #region Ctor
        public ProductConfigViewModel(IWindowService windowService, ConnStore conn, SelectedItemForConfigStore selItemStore)
        {
            _connStore = conn;
            _windowService = windowService;
            _selItemStore = selItemStore;

            #region Commands Def assigning
            CloseProdConfigComm = new RelayCommand(o => { CloseProdConfig(); }, canExecute: o => true);
            AddItemComm = new RelayCommand(o => { AddItem(); }, canExecute: o => true);
            SaveItemComm = new RelayCommand(o => { SaveItem(); }, canExecute: o => true);
            #endregion

            #region Binding Foreign event
            _windowService.ProdConfigWindowClosedEvent += WindowService_ProdCnfigWindowCloseEvent;
            #endregion

            Departments = new List<string>();
        }
        #endregion

        #region Helper Methods
        private void ClearBindedData()
        {
            UPCEntered = "";

            ProdUPC = "";
            ProdDept = null;
            ProdDesc = "";
            ProdSecDesc = "";
            ProdCost = "";
            ProdRetail = "";
            ProdInv = "";
            ProdIsTaxable = false;

            ConfigItem = null;

            IsUPCEna = false;
            IsInvEna = false;
        }
        private void WindowService_ProdCnfigWindowCloseEvent(object sender, EventArgs e)
        {
            _windowService.CloseProdConfigWindow();
            ClearBindedData();
            //change buttons
            IsDelButtonEna = true;
            IsAddButtonEna = true;
            IsDupButtonEna = true;
            IsPOButtonEna = true;
            IsLookButtonEna = true;
        }
        #endregion

        #region Commands Def
        private void CloseProdConfig()
        {
            bool isClosed = _windowService.CloseProdConfigWindow();
            ClearBindedData();
            //change buttons
            IsDelButtonEna = true;
            IsAddButtonEna = true;
            IsDupButtonEna = true;
            IsPOButtonEna = true;
            IsLookButtonEna = true;

            if (isClosed)
            {
                return;
            }
            else
            {
                MessageBox.Show("Could Not Close Prod config window");
            }

        }
        private void AddItem()
        {
            //disable buttons
            IsDelButtonEna = false;
            IsAddButtonEna = false;
            IsDupButtonEna = false;
            IsPOButtonEna = false;
            IsLookButtonEna = false;

            ClearBindedData();
            IsUPCEna = true;
            IsInvEna = true;
        }
        private void SaveItem()
        {
            int UPCresult = 0;
            //sanitize input for SQL query
            string upc = ProdUPC;

            //check if this UPC exist in database
            string query = "SELECT COUNT(*) FROM pos_1.products WHERE prod_upc = @prod_upc";
            using (MySqlCommand command = new MySqlCommand(query, _connStore.CurrentCon))
            {
                // Set parameter value
                command.Parameters.AddWithValue("@prod_upc", upc);

                // Execute the query and get the result
                UPCresult = Convert.ToInt32(command.ExecuteScalarAsync().Result);
            }

            string dept = ProdDept;
            string desc = ProdDesc;
            string secDesc = ProdSecDesc;
            double cost;
            try
            {
                cost = Convert.ToDouble(ProdCost);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            double retail;
            try
            {
                retail = Convert.ToDouble(ProdRetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            double inv;
            try
            {
                inv = Convert.ToDouble(ProdInv);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            bool isTaxable = ProdIsTaxable;

            if(UPCresult > 0 && IsAddButtonEna == false) //user is adding new product
            {
                MessageBox.Show("This product already exist");
                return;
            }
            else if(UPCresult > 0 && IsAddButtonEna == true) //user is editing product
            {
                MessageBox.Show("Product will be saved");
                IsUPCEna = false;
                IsInvEna = false;
                return;
            }
            else if(UPCresult <= 0 && IsAddButtonEna == false)
            {
                ItemForConfig item = new ItemForConfig(upc, dept, desc, secDesc, cost, retail, inv, isTaxable);

                //insert data to database
                string sqlQuery = "INSERT INTO pos_1.products (prod_upc, prod_dept, prod_desc, prod_sec_desc, prod_cost, prod_retail, prod_inv, prod_istaxable) " +
                                  "VALUES (@prod_upc, @prod_dept, @prod_desc, @prod_sec_desc, @prod_cost, @prod_retail, @prod_inv, @prod_istaxable);";
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(sqlQuery, _connStore.CurrentCon))
                    {
                        Int16 isTaxableBit = (short)((isTaxable == false) ? 0 : 1);
                        cmd.Parameters.AddWithValue("@prod_upc", upc);
                        cmd.Parameters.AddWithValue("@prod_dept", dept);
                        cmd.Parameters.AddWithValue("@prod_desc", desc);
                        cmd.Parameters.AddWithValue("@prod_sec_desc", secDesc);
                        cmd.Parameters.AddWithValue("@prod_cost", cost);
                        cmd.Parameters.AddWithValue("@prod_retail", retail);
                        cmd.Parameters.AddWithValue("@prod_inv", inv);
                        cmd.Parameters.AddWithValue("@prod_istaxable", isTaxableBit);

                        var rowEffected = cmd.ExecuteNonQueryAsync().Result;
                        MessageBox.Show(string.Format("Added {0} item", rowEffected));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    _windowService.CloseProdConfigWindow();
                    return;
                }

                ConfigItem = item;

                IsUPCEna = false;
                IsInvEna = false;

                //fix buttons
                IsDelButtonEna = true;
                IsAddButtonEna = true;
                IsDupButtonEna = true;
                IsPOButtonEna = true;
                IsLookButtonEna = true;

                return;
            }
            else
            {
                MessageBox.Show("Error while editing prouct");
                ClearBindedData();
                //fix buttons
                IsDelButtonEna = true;
                IsAddButtonEna = true;
                IsDupButtonEna = true;
                IsPOButtonEna = true;
                IsLookButtonEna = true;
            }
        }

        #endregion

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
            else
            {
                try
                {
                    string sql = "SELECT * " +
                                 "FROM pos_1.products " +
                                 "WHERE prod_upc = " + UPCEntered + ";";

                    using (MySqlCommand cmd = new MySqlCommand(sql, _connStore.CurrentCon))
                    {
                        using (var rdr = cmd.ExecuteReaderAsync().Result)
                        {
                            try
                            {
                                rdr.Read();
                                string upc = rdr[0].ToString();
                                string dept = rdr[1].ToString();
                                string desc = rdr[2].ToString();
                                string secDesc = rdr[3].ToString();
                                double cost = Convert.ToDouble(rdr[4].ToString());
                                double retail = Convert.ToDouble(rdr[5].ToString());
                                double inv = Convert.ToDouble(rdr[6].ToString());
                                bool isTaxable = (rdr[7].ToString() == "0") ? false : true;

                                ItemForConfig item = new ItemForConfig(upc, dept, desc, secDesc, cost, retail, inv, isTaxable);

                                
                                //ConfigItem = item;
                                ProdUPC = upc;
                                ProdDept = dept;
                                ProdDesc = desc;
                                ProdSecDesc = secDesc;
                                ProdCost = cost.ToString();
                                ProdRetail = retail.ToString();
                                ProdInv = inv.ToString();
                                ProdIsTaxable = isTaxable;

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                UPCEntered = "";
                            }
                        }
                    }

                    UPCEntered = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    UPCEntered = "";
                }
            }
        }
        #endregion
    }
}
