using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using OpenCvSharp;
using Simscop.Spindisk.WPF.Views;

namespace Simscop.Spindisk.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mat CurrentFrame;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"ERROR: {e}");
        }

        protected override void OnActivated(EventArgs e)
        {

        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {

        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            //var view = new ShellView()
            //{
            //    //DataContext = new ShellViewModel(),
            //};
            //view.Show();

            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Simscop.Spindisk.WPF",out bool ret);
            if (!ret)
            {
                Environment.Exit(1);
            }
            else
            {
                ShellView.Instance.Show();
            }    
        }

        public static class Motor
        {

        }
    }


}
