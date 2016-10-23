/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WPFClient.ViewModels"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using WPFClient.Navigation;

namespace WPFClient.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {

        public static FrameNavigationService NavigationService;
        public static SVC.ChatClient Proxy;
        public static SVC.Client LocalClient;
        public static MainPageViewModel MainPage;
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SetupNavigation();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<MainPageViewModel>();
        }


        private static void SetupNavigation()
        {
            NavigationService = new FrameNavigationService();
            NavigationService.Configure("LoginPage", new Uri("../Views/LoginPage.xaml", UriKind.Relative));
            NavigationService.Configure("MainPage", new Uri("../Views/MainPage.xaml", UriKind.Relative));

            SimpleIoc.Default.Register<IFrameNavigationService>(() => NavigationService);
        }
        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
    }
}