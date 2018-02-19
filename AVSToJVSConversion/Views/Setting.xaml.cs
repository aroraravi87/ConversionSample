using System.Windows;
/*****************************************************************************
File Name:              (MainWindow.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/
using AVSToJVSConversion.ViewModel;

namespace AVSToJVSConversion.Views
{

    public partial class Setting : Window,IDialog
    {
        public Setting()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }


        public Model.SettingModel ObjSettingModel { get; set; }

    }
}
