using POS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    public class DateTimeViewModel : ObservableObject
    {
        private DateTime _dt;
        private Timer _timer;
        

        public DateTime DateTime
        {
            get { return _dt; }
            set
            {
                if (_dt != value)
                {
                    _dt = value;
                    OnPropertyChanged();
                }
            }
        }


        public DateTimeViewModel()
        {
            DateTime = DateTime.Now;

            // Update the DateTime property every 5 second.
            _timer = new Timer(new TimerCallback((s) => DateTime = DateTime.Now),
                               null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }
    }
}
