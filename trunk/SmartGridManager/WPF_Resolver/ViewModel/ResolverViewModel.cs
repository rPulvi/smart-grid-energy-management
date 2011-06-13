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
        private string _enTh;

        private int _numProducers = 0;
        private int _numConsumers = 0;
        int i = 0;

        private float _enProduced;
        private float _enConsumed;
        private double _enThroughput;

        #endregion

        #region Objects
        private ObservableDictionary<DateTime, float> _enTimeLine = new ObservableDictionary<DateTime, float>();
        private ObservableDictionary<string, float> _enProdBar = new ObservableDictionary<string, float>();
        private ObservableDictionary<string, float> _enConsBar = new ObservableDictionary<string, float>();
        private ObservableDictionary<string, int> _pieList = new ObservableDictionary<string, int>();

        private ObservableCollectionEx<TempBuilding> peerList = new ObservableCollectionEx<TempBuilding>();
        private DispatcherTimer _timelineTemp;
        private DispatcherTimer _UIRefresh;
        private DispatcherTimer _clockBar;
        private BackgroundWorker _bw = new BackgroundWorker();                
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
            _enTimeLine.Add(DateTime.Now, 0f);
            OnPropertyChanged("GetPointTimeLine");

            _enTh = "En. Throughput: 0%";
            OnPropertyChanged("EnThroughput");

            _pieList.Add("Producers", 0);
            _pieList.Add("Consumers", 0);
            OnPropertyChanged("GetPieChartData");            

            _enProdBar.Add("En.Prod.", 0f);
            _enConsBar.Add("En.Cons.", 0f);
            
            OnPropertyChanged("GetEnProducedBar");
            OnPropertyChanged("GetEnConsumedBar");

            #region BackGroundWorkers
            
            _bw.WorkerReportsProgress = true;
            _bw.WorkerSupportsCancellation = true;

            _bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            _bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            _resolver = new Resolver.Resolver();
            #endregion

            #region timing
            _UIRefresh = new DispatcherTimer();
            _UIRefresh.Interval = new TimeSpan(0, 0, 5);
            _UIRefresh.Tick += new EventHandler(Temporizzatore_Tick);

            _timelineTemp = new DispatcherTimer();
            _timelineTemp.Interval = new TimeSpan(0, 0, 10);
            _timelineTemp.Tick += new EventHandler(TimeLine_Tick);

            _clockBar = new DispatcherTimer();
            _clockBar.Interval = new TimeSpan(0, 0, 1);
            _clockBar.Tick += new EventHandler(clockBar_Tick);
            #endregion

            _visStatus = Visibility.Hidden;
            _ipHost = Dns.GetHostByName(Dns.GetHostName());

            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        public ObservableDictionary<string, int> GetPieChartData
        {
            get { return _pieList; }
            set
            {
                _pieList = value;
                OnPropertyChanged("GetPieChartData");
            }
        }

        public ObservableDictionary<string, float> GetEnProducedBar
        {
            get { return _enProdBar; }
            set
            {
                _enProdBar = value;
                OnPropertyChanged("GetEnProducedBar");
            }
        }

        public ObservableDictionary<string, float> GetEnConsumedBar
        {
            get { return _enConsBar; }
            set 
            {
                _enConsBar = value;
                OnPropertyChanged("GetEnConsumedBar");
            }
        }

        public ObservableCollectionEx<TempBuilding> PeerList
        {
            get { return peerList; }
        }

        public ObservableDictionary<DateTime, float> GetPointTimeLine
        {
            get { return _enTimeLine; }
            set
            {
                _enTimeLine = value;
                OnPropertyChanged("GetPointTimeLine");
            }
        }

        public string EnThroughput
        {
            get { return _enTh; }
            set
            {
                _enTh = value;
                OnPropertyChanged("EnThroughput");
            }
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

            _resolverName = "Starting...";
            this.OnPropertyChanged("GetResolverName");


            if (_bw.IsBusy != true)
            {
                _bw.RunWorkerAsync();
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

            _enProduced = 0;
            _enConsumed = 0;

            _enThroughput = 0f;

            peerList = _resolver.GetConnectedPeers();
            OnPropertyChanged("PeerList");
            
            for(int i=0;i< peerList.Count;i++)
            {
                _enProduced += peerList[i].EnProduced;
                _enConsumed += peerList[i].EnPeak;

                #region checkStatus
                if (peerList[i].status == PeerStatus.Producer)
                    _numProducers++;

                if (peerList[i].status == PeerStatus.Consumer)
                    _numConsumers++;
                #endregion
            }

            if (_enConsumed > 0)
                _enThroughput = (_enProduced / _enConsumed) * 100;
            else
                _enThroughput = 0;

            _enTh ="En. Throughput: " + Math.Round(_enThroughput, 2) + "%";

            _pieList["Producers"] = _numProducers;
            _pieList["Consumers"] = _numConsumers;

            _enProdBar["En.Prod."] = _enProduced;
            _enConsBar["En.Cons."] = _enConsumed;

            OnPropertyChanged("GetPieChartData");
            
            OnPropertyChanged("GetEnProducedBar");
            OnPropertyChanged("GetEnConsumedBar");

            OnPropertyChanged("EnThroughput");
        }

        private void TimeLine_Tick(object sender, EventArgs e)
        {
            float enProd = 0f;

            for(int i=0;i< peerList.Count;i++)
            {
                enProd += peerList[i].EnProduced;
            }

            _enTimeLine.Add(DateTime.Now, enProd);

            OnPropertyChanged("GetPointTimeLine");
        }

        private void clockBar_Tick(object sender, EventArgs e)
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
            _clockBar.Start();
            _UIRefresh.Start();
            _timelineTemp.Start();

            _resolverName = _resolver.name;
            _resolverStatus = "Online...";
            _visStatus = Visibility.Visible;

            this.OnPropertyChanged("GetResolverName");         
            this.OnPropertyChanged("GetResolverStatus");
            this.OnPropertyChanged("ImgVisibility");
            this.OnPropertyChanged("GetResolverIP");
        }
    }
}