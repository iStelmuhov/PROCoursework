using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WPFClient.Navigation;

namespace WPFClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private RelayCommand _loadedCommand;

        public RelayCommand LoadedCommand
        {
            get
            {
                return  _loadedCommand
                    ?? ( _loadedCommand = new RelayCommand(
                    () =>
                    {
                        ViewModelLocator.NavigationService.NavigateTo("LoginPage");
                    }));
            }
        }
    }
}