using POS.Core;
using POS.MVVM.Models;
using POS.MVVM.View;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    internal class CustFaceViewModel : Core.ViewModel
    {
        private ReceiptItemsStore _recptItemsStore;
        private INavigationService _navigation;

        public INavigationService Navigation
        {
            get => _navigation;
            set
            {
                _navigation = value;
                OnPropertyChanged();
            }
        }

        public ReceiptItemsStore RecptItemsStore
        {
            get { return _recptItemsStore; }
            
        }

        public CustFaceViewModel(ReceiptItemsStore receiptItemsStore, INavigationService navigation)
        {
            _recptItemsStore = receiptItemsStore;
            Navigation = navigation;

            Navigation.ChangeCustFaceView<CustFaceTotalViewModel>();
        }
    }
}
