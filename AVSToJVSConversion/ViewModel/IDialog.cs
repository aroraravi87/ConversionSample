

namespace AVSToJVSConversion.ViewModel
{

    using System;
    using System.Windows;
    using Model;

    public interface IDialog
    {
        object DataContext { get; set; }
        bool? DialogResult { get; set; }
        Window Owner { get; set; }
        void Close();
        bool? ShowDialog();

        SettingModel ObjSettingModel { get; set; }   
    }

    public interface IDialogService
    {
        void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog;

        bool? ShowDialog<TViewModel, TModel>(TViewModel viewModel, TModel model) where TViewModel : IDialogRequestClose;

    }


    public interface IDialogRequestClose
    {
        event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

   


 

}
