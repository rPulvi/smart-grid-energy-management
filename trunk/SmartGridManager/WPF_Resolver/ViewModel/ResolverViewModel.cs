﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SmartGridManager;
using WPF_Resolver.Command;
using Resolver;

namespace WPF_Resolver.ViewModel
{
    class ResolverViewModel : ViewModelBase
    {
        List<Building> peerlist = new List<Building>();

        Resolver.Resolver r;
               
        public DelegateCommand StartResolver { get; set; }
        public DelegateCommand Exit { get; set; }


        public ResolverViewModel()
        {
            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        //quando un peer si avvia, chiama questo metodo per aggiungersi in lista, 
        //notificando il cambiamento nella ListView
        public void addPeer(Building peer)
        {
            peerlist.Add(peer);
            this.OnPropertyChanged("PeerList");
        }

        public List<Building> PeerList
        {
            get { return peerlist; }
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