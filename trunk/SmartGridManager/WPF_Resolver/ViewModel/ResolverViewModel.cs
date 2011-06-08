using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SmartGridManager;
using WPF_Resolver.Command;
using Resolver;
using System.Threading;
using System.Net;
using SmartGridManager.Core.Commons;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace WPF_Resolver.ViewModel
{
    class ResolverViewModel : ViewModelBase
    {
        #region Attributes
        private string _resolverName;
        private string _resolverStatus;
        private string _resolverIP;
        private string _ora;
        private string _minuto;
        private string _secondo;        

        int i = 0;

        #endregion

        #region Objects
        private ObservableCollectionEx<TempBuilding> peerList = new ObservableCollectionEx<TempBuilding>();
        private DispatcherTimer temporizzatore;
        private BackgroundWorker bw = new BackgroundWorker();
        private BackgroundWorker _backgroundTimer = new BackgroundWorker();
        private Resolver.Resolver _resolver;
        private Visibility _visStatus = new Visibility();
        private IPHostEntry _ipHost;
        #endregion

        #region DelegateCommands
        public DelegateCommand StartResolver { get; set; }
        public DelegateCommand Exit { get; set; }
        #endregion

        public ResolverViewModel()
        {
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            _backgroundTimer.WorkerReportsProgress = true;
            _backgroundTimer.WorkerSupportsCancellation = true;

            _backgroundTimer.DoWork += new DoWorkEventHandler(_backgroundTimer_DoWork);

            _resolver = new Resolver.Resolver();

            temporizzatore = new DispatcherTimer();
            temporizzatore.Interval = new TimeSpan(0, 0, 5);
            temporizzatore.Tick += new EventHandler(Temporizzatore_Tick);

            _visStatus = Visibility.Hidden;
            _ipHost = Dns.GetHostByName(Dns.GetHostName());

            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        void _backgroundTimer_DoWork(object sender, DoWorkEventArgs e)
        {
            DispatcherTimer clockBar = new DispatcherTimer();
            temporizzatore.Interval = new TimeSpan(0, 0, 1);
            temporizzatore.Tick += new EventHandler(cloBar_Tick);
        }

        public ObservableCollectionEx<TempBuilding> PeerList
        {
            get { return peerList; }
        }

        private bool canStart
        {
            get { return true; }
        }

        public void Start()
        { 
            _resolverName = "";
            _resolverStatus = "";
            _resolverIP = "IP:  " + _ipHost.AddressList[0].ToString();

            _resolverName = "Connecting...";
            this.OnPropertyChanged("GetResolverName");

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
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

        public Visibility ImgVisibility
        {
            get { return _visStatus; }
            set
            {
                _visStatus = value;
                OnPropertyChanged("ImgVisibility");
            }
        }

        public string GetResolverIP
        {
            get { return _resolverIP; }
            set
            {
                _resolverIP = value;
                OnPropertyChanged("GetResolverIP");
            }
        }

        public string GetOra
        {
            get { return _ora; }
            set
            {
                _ora = value;
                OnPropertyChanged("GetOra");
            }
        }

        public string GetMinuto
        {
            get { return _minuto; }
            set
            {
                _minuto = value;
                OnPropertyChanged("GetMinuto");
            }
        }

        public string GetSecondo
        {
            get { return _secondo; }
            set
            {
                _secondo = value;
                OnPropertyChanged("GetSecondo");
            }
        }

        private void Temporizzatore_Tick(object sender, EventArgs e)
        {
            peerList = _resolver.GetConnectedPeers();
            OnPropertyChanged("PeerList");                        
        }


        private void cloBar_Tick(object sender, EventArgs e)
        {
            //i += 1;
            i++;

            int ora = i / 3600;
            int minuto = i / 60;
            int secondo = i % 60;

            _ora = ora.ToString("00") + ":";
            _minuto = minuto.ToString("00") + ":";
            _secondo = secondo.ToString("00");

            OnPropertyChanged("GetOra");
            OnPropertyChanged("GetMinuto");
            OnPropertyChanged("GetSecondo");
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {            
            _resolver.Connect();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _resolverName = _resolver.name;
            this.OnPropertyChanged("GetResolverName");

            if (bw.IsBusy != true)
            {
                _backgroundTimer.RunWorkerAsync();
            }

            temporizzatore.Start();

            _resolverStatus = "Online...";
            _visStatus = Visibility.Visible;

            this.OnPropertyChanged("GetResolverStatus");
            this.OnPropertyChanged("ImgVisibility");
            this.OnPropertyChanged("GetResolverIP");            
        }
    }
}