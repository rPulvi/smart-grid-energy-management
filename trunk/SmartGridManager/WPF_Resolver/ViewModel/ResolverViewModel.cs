﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SmartGridManager;
using WPF_Resolver.Command;
using Resolver;
using System.Threading;

namespace WPF_Resolver.ViewModel
{
    class ResolverViewModel : ViewModelBase
    {
        private Thread thResolver;
        private Resolver.Resolver r;
        private string _resolverName;
        private string _resolverStatus;

        List<Building> peerlist = new List<Building>();

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
            _resolverName = "";
            _resolverStatus = "";

            r = new Resolver.Resolver();

            _resolverName = r.name;
            this.OnPropertyChanged("GetResolverName");

            thResolver = new Thread(r.Connect) { IsBackground = true };
            thResolver.Start();
            thResolver.Join();

            _resolverStatus = "Online...";
            this.OnPropertyChanged("GetResolverStatus");

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

        public string GetResolverName
        {
            get { return _resolverName; }
            set
            {
                _resolverName = value;
                OnPropertyChanged("GetResolverName");
            }
        }

        public string GetResolverStatus
        {
            get { return _resolverStatus; }
            set
            {
                _resolverStatus = value;
                OnPropertyChanged("GetResolverStatus");
            }
        }
    }
}

//Un servizio WCF può essere hostato dentro un WPF ???