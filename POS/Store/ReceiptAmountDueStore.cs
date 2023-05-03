using System.Collections.ObjectModel;

namespace POS.Store
{
    public class ReceiptAmountDueStore : Core.ObservableObject
    {
        private ObservableCollection<double> _recptDueAmount;

        public ObservableCollection<double> RecptDueAmount
        {
            get { return _recptDueAmount; }
            set
            {
                _recptDueAmount = value;
                OnPropertyChanged(nameof(RecptDueAmount));
            }
        }

        public ReceiptAmountDueStore() 
        {
            //RecptAmounts[0] -> Total
            //RecptAmounts[1] -> Amount tendered
            //RecptAmounts[2] -> Due
            //RecptAmounts[3] -> Change
            RecptDueAmount = new ObservableCollection<double>
            {
                0.00,
                0.00,
                0.00,
                0.00
            };
        }
    }
}
