using POS.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    public class CustFaceTotalViewModel : Core.ViewModel
    {
        private ReceiptAmountStore _recptAmountStore;

        public ReceiptAmountStore RecptAmountStore
        {
            get { return _recptAmountStore; }
            set
            {
                _recptAmountStore = value;
                OnPropertyChanged();
            }
        }
        public CustFaceTotalViewModel(ReceiptAmountStore recptAmountStore) 
        {
            RecptAmountStore = recptAmountStore;
        }
    }
}
