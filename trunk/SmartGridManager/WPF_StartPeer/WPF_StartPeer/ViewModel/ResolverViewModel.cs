using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPF_StartPeer.ViewModel;
using WPF_StartPeer.Command;
using System.Windows;
using SmartGridManager;

namespace WPF_Resolver.ViewModel
{
    class ResolverViewModel : ViewModelBase
    {
        Resolver.Resolver r;

        public DelegateCommand StartResolver { get; set; }
        public DelegateCommand Exit { get; set; }

        public ResolverViewModel()
        {
            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        private bool canStart
        {
            get { return true; }
        }

        public void Start()
        {
            r = new Resolver.Resolver();

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
        }

        private bool canExit
        {
            get { return true; }
        }

        public void AppExit()
        {
            Application.Current.Shutdown();
        }
    }
}

//Un servizio WCF può essere hostato dentro un WPF ???