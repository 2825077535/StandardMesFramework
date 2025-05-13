using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Prism.DryIoc;
using DryIoc;
using Prism.Services.Dialogs;

namespace MesUI.ViewModelBase
{
    public abstract class DialogViewModelBase : BindableBase, IDialogAware, IDisposable
    {
        protected DialogViewModelBase()
        {
            Container = ContainerLocator.Container.GetContainer();
            CloseCommand = new DelegateCommand<MessageBoxResult?>(CloseCommand_Execute, CloseCommand_CanExecute);
        }

        event Action<IDialogResult> IDialogAware.RequestClose
        {
            add
            {
            }

            remove
            {
            }
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
            //var programContainer = ContainerLocator.Current.Resolve<IProgramContainer>();
            //programContainer.Current.IsActive = true;
            Dispose();
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            //var programContainer = ContainerLocator.Current.Resolve<IProgramContainer>();
            //programContainer.Current.IsActive = false;
        }

        protected virtual bool CloseCommand_CanExecute(MessageBoxResult? result)
        {
            return true;
        }

        protected virtual void CloseCommand_Execute(MessageBoxResult? result)
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public virtual void Dispose()
        {

        }

        public virtual void Loaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        public DelegateCommand<MessageBoxResult?> CloseCommand { get; }

        public Action<IDialogResult> RequestClose { get; set; }

        public virtual string Title { get; set; } = string.Empty;

        protected readonly IContainer Container;

    }
}
