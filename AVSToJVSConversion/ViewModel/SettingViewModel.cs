

using System.Windows.Media;

namespace AVSToJVSConversion.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Common;
    using Model;

    public class SettingViewModel : INotifyPropertyChanged, IDialogRequestClose
    {

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        public ICommand CloseCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand SelectCommand { get; set; }
        private string _excelPath;

        public string ExcelPath
        {
            get { return _excelPath; }
            set
            {
                _excelPath = value;
                RaisedPropertyChanged("ExcelPath");
            }
        }

        private string _connectionPath;

        public string ConnectionPath
        {
            get { return _connectionPath; }
            set
            {
                _connectionPath = value;
                RaisedPropertyChanged("ConnectionPath");
            }
        }

        private string _logPath;

        public string LogPath
        {
            get { return _logPath; }
            set
            {
                _logPath = value;
                RaisedPropertyChanged("LogPath");
            }
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                RaisedPropertyChanged("ServerName");
            }
        }

        private string _databaseName;

        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                _databaseName = value;
                RaisedPropertyChanged("DatabaseName");
            }
        }


        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisedPropertyChanged("Message");
            }
        }


        private bool _updateStatus;

        public bool UpdateStatus
        {
            get { return _updateStatus; }
            set
            {
                _updateStatus = value;
                RaisedPropertyChanged("UpdateStatus");
            }
        }


        public SettingViewModel()
        {
            SelectCommand = new RelayCommand(OnActionExecute);
            UpdateCommand =
                new RelayCommand(
                    p =>
                        CloseRequested.Invoke(this,
                            new DialogCloseRequestedEventArgs(true,
                                new SettingModel()
                                {
                                    LogPath = this.LogPath,
                                    ExcelPath = this.ExcelPath,
                                    ConnectionPath = this.ConnectionPath
                                })));
            CloseCommand =
                new RelayCommand(
                    p =>
                        CloseRequested.Invoke(this,
                            new DialogCloseRequestedEventArgs(false,
                                new SettingModel()
                                {
                                    LogPath = this.LogPath,
                                    ExcelPath = this.ExcelPath,
                                    ConnectionPath = this.ConnectionPath
                                })));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properyName"></param>
        private void RaisedPropertyChanged(string properyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(properyName));
            }
        }


        public bool SaveSetting()
        {
            try
            {
                if (ValidateFields())
                {
                    Helpers.UpdateAppSettingFile(new SettingModel()
                    {
                        LogPath = LogPath,
                        ExcelPath = ExcelPath,
                        ConnectionPath =
                            string.Format(@"Data Source={0};Initial Catalog={1};Integrated Security=true;", ServerName,
                                DatabaseName)
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                
            }
            return false;
        }


        private void OnActionExecute(object Parameter)
        {

            switch (Parameter.ToString())
            {
                case "ExcelPath":

                    using (var fileDialog = new System.Windows.Forms.OpenFileDialog())
                    {
                        fileDialog.DefaultExt = ".xlsx";
                        fileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                        fileDialog.ShowDialog();
                        ExcelPath = fileDialog.FileName;

                    }
                    break;
                case "LogPath":
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        LogPath = folderDialog.SelectedPath;
                    }
                    break;
            }
        }

        public event
            PropertyChangedEventHandler PropertyChanged;


        public bool ValidateFields()
        {
            bool isSuccess;
            if (!string.IsNullOrEmpty(ServerName) && !string.IsNullOrEmpty(DatabaseName) && !string.IsNullOrEmpty(ExcelPath) &&!string.IsNullOrEmpty(LogPath))
            {
                isSuccess = true;
                   
            }
            else
            {
                Message = "All Fields are Mandatory";
                isSuccess = false;
            }
            UpdateStatus = isSuccess;
            return isSuccess;
        }
    }
}