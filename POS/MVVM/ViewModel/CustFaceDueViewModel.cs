using POS.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    public class CustFaceDueViewModel : Core.ViewModel
    {
        private ReceiptAmountDueStore _recptAmtDueStore;

        public ReceiptAmountDueStore RecptAmtDueStore
        {
            get { return _recptAmtDueStore; }
            set
            { 
                _recptAmtDueStore = value;
                OnPropertyChanged(nameof(RecptAmtDueStore));
            }
        }
        public CustFaceDueViewModel(ReceiptAmountDueStore recptAmtDueStore)
        {
            RecptAmtDueStore = recptAmtDueStore;
        }
    }
}
