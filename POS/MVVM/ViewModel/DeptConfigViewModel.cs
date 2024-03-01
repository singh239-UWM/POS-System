using MySql.Data.MySqlClient;
using POS.Core;
using POS.Services;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS.MVVM.ViewModel
{
    public class DeptConfigViewModel : Core.ViewModel
    {
        private ConnStore _connStore;
        private IWindowService _windowService;

        private List<string> _departments;
        private string _selectedDept;
        private string _deptDesc;
        private bool _isDeptListEna = true;
        private bool _isDeptDescFieldEna = false;
        private bool _isSaveButtonEna = false;
        private bool _isAddButtonEna = true;

        public List<string> Departments
        {
            get
            {
                List<string> list = new List<string>();
                string sql = "SELECT dept_desc FROM pos_1.departments;";
                using (MySqlCommand cmd = new MySqlCommand(sql, _connStore.CurrentCon))
                {
                    using (var rdr = cmd.ExecuteReaderAsync().Result)
                    {
                        while (rdr.Read())
                        {
                            list.Add((string)rdr[0]);
                        }
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
        public string SelectedDept
        {
            get { return _selectedDept; }
            set
            {
                _selectedDept = value;
                DeptDesc = value;
                OnPropertyChanged();
            }
        }
        public string DeptDesc
        {
            get { return _deptDesc; }
            set
            {
                _deptDesc = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeptListEna
        {
            get { return _isDeptListEna; }
            set
            {
                _isDeptListEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeptDescFieldEna
        {
            get { return _isDeptDescFieldEna; }
            set
            {
                _isDeptDescFieldEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsSaveButtonEna
        {
            get { return _isSaveButtonEna; }
            set
            {
                _isSaveButtonEna = value;
                OnPropertyChanged();
            }
        }
        public bool IsAddButtonEna
        {
            get { return _isAddButtonEna; }
            set
            {
                _isAddButtonEna = value;
                OnPropertyChanged();
            }
        }


        #region command definitions
        public RelayCommand CloseDeptConfigComm { get; set; }
        public RelayCommand AddDeptComm { get; set; }
        public RelayCommand SaveDeptComm { get; set; }
        #endregion

        public DeptConfigViewModel(IWindowService windowService, ConnStore conn)
        {
            _connStore = conn;
            _windowService = windowService;

            #region Commands Def assigning
            CloseDeptConfigComm = new RelayCommand(o => { CloseDeptConfig(); }, canExecute: o => true);
            AddDeptComm = new RelayCommand(o => { AddDept(); }, canExecute: o => true);
            SaveDeptComm = new RelayCommand(o => { SaveDept(); }, canExecute: o => true);
            #endregion

        }

        #region Commands Def
        private void CloseDeptConfig()
        {
            bool isClosed = _windowService.CloseDeptConfigWindow();

            if (isClosed)
            {
                SelectedDept = null;

                IsDeptListEna = true;
                IsDeptDescFieldEna = false;
                IsSaveButtonEna = false;
                IsAddButtonEna = true;
                return;
            }
            else
            {
                MessageBox.Show("Could Not Close Dept config window");
            }

        }
        private void AddDept()
        {
            IsDeptListEna = false;
            IsDeptDescFieldEna = true;
            IsSaveButtonEna = true;
            IsAddButtonEna = false;

            SelectedDept = null;
        }
        private void SaveDept()
        {
            if (DeptDesc == "") { MessageBox.Show("Invalid Dept Description"); return; }
            DeptDesc = DeptDesc.Trim();
            if (DeptDesc == "") { MessageBox.Show("Invalid Dept Description"); return; }

            string sqlInsertQuery = "INSERT INTO pos_1.departments (dept_desc) " +
                                    "VALUE (@dept_desc);";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sqlInsertQuery, _connStore.CurrentCon))
                {
                    cmd.Parameters.AddWithValue("@dept_desc", DeptDesc);

                    var rowEffected = cmd.ExecuteNonQueryAsync().Result;
                }

                IsDeptListEna = true;
                IsDeptDescFieldEna = false;
                IsSaveButtonEna = false;
                IsAddButtonEna = true;

                Departments = Departments;
                SelectedDept = DeptDesc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.Message);
                return;
            }
        }
        #endregion
    }
}
