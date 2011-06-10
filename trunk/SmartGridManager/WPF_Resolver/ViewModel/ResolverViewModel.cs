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

        private int _numProducers = 0;
        private int _numConsumers = 0;
        int i = 0;

        #endregion

        #region Objects
        private ObservableCollectionEx<PieItem> _pieList = new ObservableCollectionEx<PieItem>();
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
            _pieList.Add(new PieItem(){key = "Producers", value = 0});
            _pieList.Add(new PieItem() { key = "Consumers", value = 0 });
            OnPropertyChanged("GetPieChartData");

            #region BackGroundWorkers
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            _backgroundTimer.WorkerReportsProgress = true;
            _backgroundTimer.WorkerSupportsCancellation = true;

            _backgroundTimer.DoWork += new DoWorkEventHandler(_backgroundTimer_DoWork);

            _resolver = new Resolver.Resolver();
            #endregion

            #region timing
            temporizzatore = new DispatcherTimer();
            temporizzatore.Interval = new TimeSpan(0, 0, 5);
            temporizzatore.Tick += new EventHandler(Temporizzatore_Tick);
            #endregion

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

        public ObservableCollectionEx<PieItem> GetPieChartData
        {
            get { return _pieList; }
            set
            {
                _pieList = value;
                OnPropertyChanged("GetPieChartData");
            }
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
            _numProducers = 0;
            _numConsumers = 0;

            peerList = _resolver.GetConnectedPeers();
            OnPropertyChanged("PeerList");

            //foreach (var p in peerList)
            //{
            //    if (p.status == PeerStatus.Producer)
            //    {
            //        _numProducers++;

            //        _pieList["Producers"] = _numProducers;
            //    }

            //    if (p.status == PeerStatus.Consumer)
            //    {
            //        _numConsumers++;

            //        _pieList["Consumers"] = _numConsumers;
            //    }
            //}

            for (int j = 0; j < peerList.Count(); j++)
            {
                if (peerList[j].status == PeerStatus.Producer)
                {
                    _numProducers++;

                    //_pieList["Producers"] = _numProducers;
                    _pieList[0].value = _numProducers;
                }

                if (peerList[j].status == PeerStatus.Consumer)
                {
                    _numConsumers++;

                    _pieList[1].value = _numConsumers;
                }
            }

            OnPropertyChanged("GetPieChartData");
        }

        private void cloBar_Tick(object sender, EventArgs e)
        {
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

    public class PieItem
    {
        public string key { get; set; }
        public int value { get; set; }
    }
}