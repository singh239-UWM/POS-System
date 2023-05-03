using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Core
{
    public interface INavigationService
    {
        ViewModel CurrentView { get; }
        ViewModel CartVM { get; }
        ViewModel CustFaceVM { get; }
        ViewModel DashboardTabVM { get; }
        void NavigateTo<T>() where T : ViewModel;
        void ViewCartView<T>() where T : ViewModel;
        void ChangeCustFaceView<T>() where T : ViewModel;
        void NavigatDashBoardTabTo<T>() where T : ViewModel;
    }
}
