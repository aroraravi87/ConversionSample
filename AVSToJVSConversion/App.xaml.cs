

using System;
using System.Threading;

namespace AVSToJVSConversion
{
    using System.Windows;
    using ViewModel;
    using Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;
        private bool mutexCreated;
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            IDialogService dialogService = new DialogService(MainWindow);
            dialogService.Register<SettingViewModel, Setting>();
            var viewmodel = new MainWindowViewModel(dialogService);
            var view = new MainWindow { DataContext = viewmodel };




            _mutex = new Mutex(true, "Global\\" + appGuid, out mutexCreated);
            if (mutexCreated)
                _mutex.ReleaseMutex();

            if (!mutexCreated)
            {
                Application.Current.Shutdown();
                return;
            }
            view.ShowDialog();
        }
    }
}
