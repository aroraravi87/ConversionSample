using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AVSToJVSConversion.BLL;
using sc = System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using AVSToJVSConversion.Common;


namespace AVSToJVSConversion.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        #region === [Properties]================================

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
        private Visibility _progressVisibility;

        public Visibility ProgressVisibility
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
        private Visibility _progressStatusVisibility;

        public Visibility ProgressStatusVisibility
        {
            get { return _progressStatusVisibility; }
            set
            {
                _progressStatusVisibility = value;
                RaisedPropertyChanged("ProgressStatusVisibility");
            }
        }
        #endregion



        #region === [Constructor]================================


        private Utility _utility = null;

        public ICommand SelectCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private BackgroundWorker backGroundWorker;
        private ICommand instigateWorkCommand { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        public MainWindowViewModel()
        {
            SelectCommand = new CustomCommand(OnActionExecute);
            backGroundWorker = new BackgroundWorker();
            _utility = new Utility();
            MlsFileCount = 0;
            ProgressVisibility = Visibility.Collapsed;
            ProgressStatusVisibility = Visibility.Collapsed;
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
                case "BrowseAVS":
                    ErrorMessage = string.Empty;
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        AVSPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseLibrary":
                    ErrorMessage = string.Empty;
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        LibraryPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseJVS":
                    ErrorMessage = string.Empty;
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
                    ValidateError.Opacity = 0;
                    ErrorMessage = string.Empty;
                    Status = false;
                    break;

            }

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

            InputProcessor inputProcessor2 = new InputProcessor();
            int filelength = Directory.EnumerateFiles(AVSPath, "*.mls").Count();
            MlsFileCount = filelength;
            ProgressVisibility = Visibility.Visible;
            _utility.GetCustomLogAppender();
            if (Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["basePath"], "FailedScripts")))
                Array.ForEach(Directory.GetFiles(Path.Combine(ConfigurationManager.AppSettings["basePath"], "FailedScripts")), File.Delete);
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

            ProgressStatusVisibility = Visibility.Visible;
            SuccessCount = Directory.GetFiles(JVSPath).Count();
            FailureCount = Directory.GetFiles(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                "FailedScripts")).Count();
            AVSPath = string.Empty;
            LibraryPath = string.Empty;
            JVSPath = string.Empty;

        }

        #endregion


    }
}

