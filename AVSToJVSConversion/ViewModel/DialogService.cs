

namespace AVSToJVSConversion.ViewModel
{

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using Model;

    public class DialogService : IDialogService
    {
        private readonly Window _owner;
        public IDictionary<Type, Type> Mappings { get; set; }

        public DialogService(Window owner)
        {
            this._owner = owner;
            Mappings = new Dictionary<Type, Type>();
        }


        public void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog
        {
            if (Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException(string.Format(@"Type {0} is already mapped to type {1}", typeof(TViewModel), typeof(TView)));
            }
            Mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public bool? ShowDialog<TViewModel, TModel>(TViewModel viewModel, TModel model) where TViewModel : IDialogRequestClose
        {
            Type viewType = Mappings[typeof(TViewModel)];
            IDialog dialog = (IDialog)Activator.CreateInstance(viewType);
            EventHandler<DialogCloseRequestedEventArgs> handler = null;
            handler = (sender, e) =>
            {
                viewModel.CloseRequested -= handler;
                if (e.DialogResult.HasValue)
                {
                    dialog.DialogResult = e.DialogResult;
                    dialog.ObjSettingModel = new SettingModel() { LogPath = e.DialogLogPath, ConnectionPath = e.DialogConnectionPath, ExcelPath = e.DialogExcelPath };
                }
                else
                {
                    dialog.Close();
                }


            };
            viewModel.CloseRequested += handler;

            if (model != null && model.GetType() == typeof(SettingModel))
            {
                SettingViewModel obj = viewModel as SettingViewModel;
                obj.ConnectionPath = (model as SettingModel).ConnectionPath;
                GetDbDetails(obj, obj.ConnectionPath);
                obj.LogPath = (model as SettingModel).LogPath;
                obj.ExcelPath = (model as SettingModel).ExcelPath;
            }
            dialog.DataContext = viewModel;
            dialog.Owner = _owner;
            return dialog.ShowDialog();
        }

        private void GetDbDetails(SettingViewModel obj, string connection)
        {
            obj.ServerName = connection.Substring(connection.IndexOf('=') + 1,
                connection.IndexOf(';') - connection.IndexOf('=') - 1);
            obj.DatabaseName = connection.Substring(connection.IndexOf('=', connection.IndexOf('=') + 1) + 1,
                connection.IndexOf(';', connection.IndexOf(';') + 1) - connection.IndexOf('=', connection.IndexOf('=') + 1) - 1);
        }


    }

    public class DialogCloseRequestedEventArgs
    {
        public DialogCloseRequestedEventArgs(bool? dialogResult, SettingModel objModel)
        {
            DialogResult = dialogResult;
            DialogLogPath = objModel.LogPath;
            DialogExcelPath = objModel.ExcelPath;
            DialogConnectionPath = objModel.ConnectionPath;
           
        }

        public bool? DialogResult { get; set; }
        public string DialogLogPath { get; set; }
        public string DialogExcelPath { get; set; }
        public string DialogConnectionPath { get; set; }
    }
}

