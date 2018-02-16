

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
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();   
            IDialogService dialogService = new DialogService(MainWindow);
            dialogService.Register<SettingViewModel,Setting>();
            var viewmodel = new MainWindowViewModel(dialogService) ;
            var view = new MainWindow {DataContext = viewmodel};
            view.ShowDialog();
          
        } 
    }
}
