using GalaSoft.MvvmLight.Views;

namespace WPFClient.Navigation
{
    public interface IFrameNavigationService : INavigationService
    {
        object Parameter { get; }
    }
}