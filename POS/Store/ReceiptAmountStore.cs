using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Store
{
    public class ReceiptAmountStore : Core.ObservableObject
    {
        private ObservableCollection<double> _recptAmount;
        private ObservableCollection<double> _unChangeRecptAmount;
        private bool _isTaxFree;
        private double _taxAmtStore;

        public ObservableCollection<double> RecptAmount
        {
            get { return _recptAmount; }
            set 
            { 
                _recptAmount = value; 
                OnPropertyChanged(nameof(RecptAmount));
            }
        }

        public ObservableCollection<double> UnChangeRecptAmount
        {
            get { return _unChangeRecptAmount; }
            set
            {
                _unChangeRecptAmount = value;
                OnPropertyChanged(nameof(UnChangeRecptAmount));
            }
        }

        public double TaxAmtStore { get => _taxAmtStore; set => _taxAmtStore = value; }
        public bool IsTaxFree { get => _isTaxFree; set => _isTaxFree = value; }
    }
}
