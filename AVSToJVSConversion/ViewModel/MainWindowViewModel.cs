
using System.Diagnostics;
using System.Net;

namespace AVSToJVSConversion.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using SW = System.Windows;
    using System.Windows.Media;
    using System.Windows.Forms;
    using System.Windows.Input;
    using BLL;
    using Model;
    using sc = System.Windows.Controls;


    using Common;


    public class MainWindowViewModel : INotifyPropertyChanged, IDialogRequestClose
    {

        #region === [Properties]================================

        public ICommand SelectCommand { get; set; }
        private ICommand instigateWorkCommand { get; set; }

        private string _AVSPath;

        public string AVSPath
        {
            get { return _AVSPath; }
            set
            {
                _AVSPath = value;
                RaisedPropertyChanged("AVSPath");
            }
        }

        private string _libraryPath;

        public string LibraryPath
        {
            get { return _libraryPath; }
            set
            {
                _libraryPath = value;
                RaisedPropertyChanged("LibraryPath");

            }
        }

        private string _jVSPath;

        public string JVSPath
        {
            get { return _jVSPath; }
            set
            {
                _jVSPath = value;
                RaisedPropertyChanged("JVSPath");
            }
        }

        private sc.Label _validateError;

        public sc.Label ValidateError
        {
            get { return _validateError; }
            set
            {
                _validateError = value;
                RaisedPropertyChanged("ValidateError");
            }
        }


        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisedPropertyChanged("ErrorMessage");
            }
        }


        private bool _status;

        public bool Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisedPropertyChanged("Status");
            }
        }

        private string _pageCountMessage;

        public string PageCountMessage
        {
            get { return _pageCountMessage; }
            set
            {
                _pageCountMessage = value;
                RaisedPropertyChanged("PageCountMessage");
            }
        }



        private int _progressStatus;

        //public int ProgressStatus
        //{
        //    get { return _progressStatus; }
        //    set
        //    {
        //        _progressStatus = value;
        //        RaisedPropertyChanged("ProgressStatus");
        //    }
        //}

        private int _mlsFileCount;

        public int MlsFileCount
        {
            get { return _mlsFileCount; }
            set
            {
                _mlsFileCount = value;
                RaisedPropertyChanged("MlsFileCount");
            }
        }
        private SW.Visibility _progressVisibility;

        public SW.Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                RaisedPropertyChanged("ProgressVisibility");
            }
        }

        private int _sucessCount;

        public int SuccessCount
        {
            get { return _sucessCount; }
            set
            {
                _sucessCount = value;
                RaisedPropertyChanged("SuccessCount");
            }
        }
        private int _failureCount;

        public int FailureCount
        {
            get { return _failureCount; }
            set
            {
                _failureCount = value;
                RaisedPropertyChanged("FailureCount");
            }
        }
        private SW.Visibility _progressStatusVisibility;

        public SW.Visibility ProgressStatusVisibility
        {
            get { return _progressStatusVisibility; }
            set
            {
                _progressStatusVisibility = value;
                RaisedPropertyChanged("ProgressStatusVisibility");
            }
        }


        private bool _close;
        public bool Close
        {
            get { return _close; }
            set
            {
                if (_close == value)
                    return;
                _close = value;
                RaisedPropertyChanged("Close");
            }
        }


        #endregion



        #region === [Constructor]================================


        private Utility _utility = null;


        public event PropertyChangedEventHandler PropertyChanged;

        private BackgroundWorker backGroundWorker;

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

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

        private readonly IDialogService _dialogService;

        /// <summary>
        /// 
        /// </summary>
        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            SelectCommand = new RelayCommand(OnActionExecute);
            backGroundWorker = new BackgroundWorker();
            _utility = new Utility();
            MlsFileCount = 0;
            ProgressVisibility = SW.Visibility.Collapsed;
            ProgressStatusVisibility = SW.Visibility.Collapsed;
            SuccessCount = 0;
            FailureCount = 0;
        }




        // your UI binds to this command in order to kick off the work
        public ICommand InstigateWorkCommand
        {
            get { return this.instigateWorkCommand; }
        }

        public int ProgressStatus
        {
            get { return this._progressStatus; }
            set
            {
                if (this._progressStatus != value)
                {
                    this._progressStatus = value;
                    RaisedPropertyChanged("ProgressStatus");
                }
            }
        }



        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressStatus = e.ProgressPercentage;
        }

        #endregion

        #region === [Private Methods] =============================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Parameter"></param>
        private void OnActionExecute(object Parameter)
        {

            switch (Parameter.ToString())
            {
                case "Setting":
                    DisplaySettingDialog();
                    break;
                case "FAQ":
                    DownloadDocument();
                    break;
                case "Help":
                    DownloadDocument();
                    break;

                case "BrowseAVS":
                    ResetFields();
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        AVSPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseLibrary":
                    ResetFields();
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        LibraryPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseJVS":
                    ResetFields();
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        JVSPath = folderDialog.SelectedPath;
                    }
                    break;
                case "Conversion":
                    ConvertAvsFiletoJvs();
                    break;
                case "Refresh":
                    AVSPath = string.Empty;
                    JVSPath = string.Empty;
                    LibraryPath = string.Empty;
                    if (ValidateError != null)
                        ValidateError.Opacity = 0;
                    ResetFields();
                    break;


            }

        }

        private void DownloadDocument()
        {
            string str = string.Format("{0}{1}", ConfigurationManager.AppSettings["DocumentPath"], "Conversion.pdf");
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = str;
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                SW.MessageBox.Show("Document not supported,Please contact to Administrator.", "Invalid Document", SW.MessageBoxButton.OKCancel, SW.MessageBoxImage.Error);
            }
        }

        private void ResetFields()
        {
            ErrorMessage = string.Empty;
            Status = false;
            ProgressVisibility = SW.Visibility.Collapsed;
            ProgressStatusVisibility = SW.Visibility.Collapsed;
        }

        private void DisplaySettingDialog()
        {
            SettingViewModel viewmodel = new SettingViewModel();

            SettingModel objSetting = BindSettingData();


            bool? result = _dialogService.ShowDialog(viewmodel, objSetting);
            if (result.HasValue)
            {
                if (result.Value)
                {

                    if (viewmodel.SaveSetting())
                    {
                        viewmodel.UpdateStatus = true;
                        viewmodel.Message = "Setting updated successfully!!";
                        objSetting = null;
                        _dialogService.ShowDialog(viewmodel, objSetting);

                    }
                    else
                    {
                        viewmodel.UpdateStatus = false;
                        DisplaySettingDialog();
                    }
                }
                else
                {

                }
            }
        }



        public SettingModel BindSettingData()
        {
            SettingModel objSettingModel = new SettingModel();
            objSettingModel.ConnectionPath = Helpers.GetSetting("dbconnection", "Web");
            objSettingModel.LogPath = Helpers.GetSetting("basePath", "App");
            objSettingModel.ExcelPath = Helpers.GetSetting("ExcelPath", "App");
            return objSettingModel;
        }





        /// <summary>
        /// 
        /// </summary>
        /// 
        private void ConvertAvsFiletoJvs()
        {
            bool IsSuccess = false;
            string emptyField = "**Missing Information :  ";
            if (string.IsNullOrEmpty(AVSPath) || string.IsNullOrEmpty(JVSPath))
            {
                if (string.IsNullOrEmpty(AVSPath))
                {
                    emptyField = emptyField + "AVS Script Path ";
                }
                if (string.IsNullOrEmpty(AVSPath) && string.IsNullOrEmpty(JVSPath))
                {
                    emptyField = emptyField + "&";
                }

                if (string.IsNullOrEmpty(JVSPath))
                {
                    emptyField = emptyField + " JVS Output Path ";
                }
                if (_validateError == null)
                {
                    _validateError = new sc.Label();
                    ErrorMessage = string.Empty;
                }
                ValidateError.Foreground = new SolidColorBrush(Colors.Red);

                ValidateError.Content = emptyField;
                ErrorMessage = emptyField;
                Status = IsSuccess;
                // ValidateError.Opacity = 1;
            }
            else
            {
                backGroundWorker = new BackgroundWorker();
                backGroundWorker.RunWorkerAsync();
                backGroundWorker.DoWork += this.DoWork;
                backGroundWorker.ProgressChanged += this.ProgressChanged;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Thread t = new Thread(InitProcess);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }


        private void InitProcess()
        {
            if (_validateError == null)
            {
                _validateError = new sc.Label();
                ErrorMessage = string.Empty;
            }
            bool IsSuccess = false;
            int counter = 0;
            int ProgressPecentage = 1;

            if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["basePath"], "Logs")))
            {
                Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                    "Logs"));
            }

            if (!Directory.Exists(Path.Combine(string.Concat(ConfigurationManager.AppSettings["basePath"], "\\Logs"), "FailedScripts")))
            {
                Directory.CreateDirectory(Path.Combine(string.Concat(ConfigurationManager.AppSettings["basePath"], "\\Logs"), "FailedScripts"));
            }
            InputProcessor inputProcessor2 = new InputProcessor();
            int filelength = Directory.EnumerateFiles(AVSPath, "*.mls").Count();
            MlsFileCount = filelength;
            ProgressVisibility = SW.Visibility.Visible;
            _utility.GetCustomLogAppender();
            int existCount = Directory.GetFiles(JVSPath).Count();
            if (Directory.Exists(Path.Combine(string.Concat(ConfigurationManager.AppSettings["basePath"], "\\Logs"), "FailedScripts")))
                Array.ForEach(Directory.GetFiles(Path.Combine(string.Concat(ConfigurationManager.AppSettings["basePath"], "\\Logs"), "FailedScripts")), File.Delete);
            foreach (string file in Directory.EnumerateFiles(AVSPath, "*.mls"))
            {

                counter++;
                // Thread.Sleep(1000);
                inputProcessor2.Process(AVSPath,
                     string.IsNullOrEmpty(LibraryPath) ? AVSPath : LibraryPath,
                     JVSPath, file);

                ProgressStatus = counter;
                ProgressPecentage =
                    Convert.ToInt32((Convert.ToDecimal(ProgressStatus) / Convert.ToDecimal(filelength)) *
                                    Convert.ToDecimal(100));
                PageCountMessage = string.Format("Status : {0}%, {1} out of {2} files completed.", ProgressPecentage,
                    ProgressStatus, filelength);
            }


            ProgressStatusVisibility = SW.Visibility.Visible;
            SuccessCount = Directory.GetFiles(JVSPath).Count() == existCount ? Directory.GetFiles(JVSPath).Count() : Directory.GetFiles(JVSPath).Count() - existCount;
            FailureCount = Directory.GetFiles(Path.Combine(string.Concat(ConfigurationManager.AppSettings["basePath"], "\\Logs"),
                "FailedScripts")).Count();
            AVSPath = string.Empty;
            LibraryPath = string.Empty;
            JVSPath = string.Empty;

        }

        #endregion


    }
}

