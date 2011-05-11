using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_StartPeer.ViewModel;
using System.Diagnostics;

namespace WPF_StartPeer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StartPeerViewModel myvm;

        public MainWindow()
        {
            InitializeComponent();

            //Creo il ViewModel
            this.myvm = new StartPeerViewModel();

            //associo Datasource
            base.DataContext = myvm;

            Trace.Listeners.Add(this.myvm);
        }
    }
}
