using DryIoc;
using MesUI.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MesUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected IContainer Container;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Container = ContainerLocator.Container.GetContainer();

            Container.Resolve<IDialogService>().ShowDialog(nameof(MesClientUIView), null, result =>
            { });
        }
    }
}
