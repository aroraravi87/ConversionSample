

namespace AVSToJVSConversion.Views
{
    using System.Windows;
    using ViewModel;
    
    public partial class MainWindow : Window,IDialog
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }


        public Model.SettingModel ObjSettingModel { get; set; }
    }
}
