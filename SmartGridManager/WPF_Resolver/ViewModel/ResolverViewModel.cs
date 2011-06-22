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
using SmartGridManager.Core.Utils;

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
        private string _imgPath;
        private string _startColour;

        private int _localFontSize;
        private int _remoteFontSize;
        private int _numProducers = 0;
        private int _numConsumers = 0;
        int i = 0;

        private float _enProduced;
        private float _enConsumed;
        private double _enThroughput;

        private bool _startIsEnabled;

        #endregion

        #region Objects

        private Visibility _listVisibilityLocal = new Visibility();
        private Visibility _listVisibilityRemote = new Visibility();
        private Visibility _getTimeVisibility = new Visibility();

        private ObservableDictionary<DateTime, float> _enTimeLine = new ObservableDictionary<DateTime, float>();
        private ObservableDictionary<string, float> _enProdBar = new ObservableDictionary<string, float>();
        private ObservableDictionary<string, float> _enConsBar = new ObservableDictionary<string, float>();
        private ObservableDictionary<string, int> _pieList = new ObservableDictionary<string, int>();

        private ObservableCollectionEx<TempBuilding> _peerList = new ObservableCollectionEx<TempBuilding>();
        private ObservableCollectionEx<RemoteHost> _hostList = new ObservableCollectionEx<RemoteHost>();
        private DispatcherTimer _timelineTemp;
        private DispatcherTimer _UIRefresh;
        private DispatcherTimer _clockBar;
        private BackgroundWorker _bw = new BackgroundWorker();                
        private Resolver.Resolver _resolver;
        private string _ipHost;
        #endregion

        #region DelegateCommands
        public DelegateCommand StartResolver { get; set; }
        public DelegateCommand Exit { get; set; }
        public DelegateCommand ShowLocal { get; set; }
        public DelegateCommand ShowRemote { get; set; }
        public DelegateCommand SetLocalFont { get; set; }
        public DelegateCommand SetRemoteFont { get; set; }
        public DelegateCommand ViewLog { get; set; }
        #endregion

        public ResolverViewModel()
        {
            #region init
            _startIsEnabled = true;
            OnPropertyChanged("GetIsEnabledStatus");

            _startColour = "Blue";
            OnPropertyChanged("GetStartColour");

            _localFontSize = 13;
            _remoteFontSize = 13;

            OnPropertyChanged("SetLocalFontSize");
            OnPropertyChanged("SetRemoteFontSize");

            _listVisibilityLocal = Visibility.Hidden;
            OnPropertyChanged("SetVisibilityLocal");

            _listVisibilityRemote = Visibility.Hidden;
            OnPropertyChanged("SetVisibilityRemote");

            _getTimeVisibility = Visibility.Hidden;
            OnPropertyChanged("GetTimeVisibility");

            _imgPath = @"/WPF_Resolver;component/img/offline.png";
            OnPropertyChanged("GetImgPath");

            _resolverStatus = "Offline...";
            OnPropertyChanged("GetResolverStatus");

            _enTimeLine.Add(DateTime.Now, 0f);
            OnPropertyChanged("GetPointTimeLine");

            _enTh = "En. Balance: 0%";
            OnPropertyChanged("EnThroughput");

            _pieList.Add("Producers", 0);
            _pieList.Add("Consumers", 0);
            OnPropertyChanged("GetPieChartData");            

            _enProdBar.Add("En.Prod.", 0f);
            _enConsBar.Add("En.Cons.", 0f);
            
            OnPropertyChanged("GetEnProducedBar");
            OnPropertyChanged("GetEnConsumedBar");
            #endregion

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

            _ipHost = Tools.getLocalIP();

            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canDo);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canDo);
            this.ShowLocal = new DelegateCommand((o) => this.ChangeVisibilityLocal(), o => this.canDo);
            this.ShowRemote = new DelegateCommand((o) => this.ChangeVisibilityRemote(), o => this.canDo);
            this.SetLocalFont = new DelegateCommand((o) => this.ChangeLocalFontSize(), o => this.canDo);
            this.SetRemoteFont = new DelegateCommand((o) => this.ChangeRemoteFontSize(), o => this.canDo);
            this.ViewLog = new DelegateCommand((o) => this.Log(), o => this.canDo);
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
            get { return _peerList; }
        }

        public ObservableCollectionEx<RemoteHost> HostList
        {
            get { return _hostList; }
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

        public void Log()
        {
            View.LogView LogWindow = new View.LogView();
            LogWindow.ShowDialog();
        }

        public Visibility SetVisibilityLocal
        {
            get { return _listVisibilityLocal; }
            set
            {
                _listVisibilityLocal = value;
                OnPropertyChanged("SetVisibility");
            }
        }

        public Visibility SetVisibilityRemote
        {
            get { return _listVisibilityRemote; }
            set
            {
                _listVisibilityRemote = value;
                OnPropertyChanged("SetVisibilityRemote");
            }
        }

        public Visibility GetTimeVisibility
        {
            get { return _getTimeVisibility; }
            set
            {
                _getTimeVisibility = value;
                OnPropertyChanged("GetTimeVisibility");
            }
        }

        public string GetStartColour
        {
            get { return _startColour; }
            set
            {
                _startColour = value;
                OnPropertyChanged("GetStartColour");
            }
        }

        public bool GetIsEnabledStatus
        {
            get { return _startIsEnabled; }
            set
            {
                _startIsEnabled = value;
                OnPropertyChanged("GetIsEnabledStatus");
            }
        }

        public int SetLocalFontSize
        {
            get { return _localFontSize; }
            set
            {
                _localFontSize = value;
                OnPropertyChanged("SetLocalFontSize");
            }
        }

        public int SetRemoteFontSize
        {
            get { return _remoteFontSize; }
            set
            {
                _remoteFontSize = value;
                OnPropertyChanged("SetRemoteFontSize");
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

        public void ChangeVisibilityLocal()
        {
            _listVisibilityLocal = Visibility.Visible;
            OnPropertyChanged("SetVisibilityLocal");

            _localFontSize = 13;
            OnPropertyChanged("SetLocalFontSize");

            _listVisibilityRemote = Visibility.Hidden;
            OnPropertyChanged("SetVisibilityRemote");
        }

        public void ChangeVisibilityRemote()
        {
            _listVisibilityRemote = Visibility.Visible;
            OnPropertyChanged("SetVisibilityRemote");

            _remoteFontSize = 13;
            OnPropertyChanged("SetRemoteFontSize");

            _listVisibilityLocal = Visibility.Hidden;
            OnPropertyChanged("SetVisibilityLocal");
        }

        public void ChangeLocalFontSize()
        {
            _localFontSize = 11;
            OnPropertyChanged("SetLocalFontSize");
        }

        public void ChangeRemoteFontSize()
        {
            _remoteFontSize = 11;
            OnPropertyChanged("SetRemoteFontSize");
        }

        public void Start()
        {
            _listVisibilityLocal = Visibility.Visible;
            _resolverName = "";
            _resolverIP = "IP:  " + _ipHost;

            _resolverName = "Starting...";
            OnPropertyChanged("GetResolverName");

            OnPropertyChanged("SetVisibilityLocal");


            if (_bw.IsBusy != true)
            {
                _bw.RunWorkerAsync();
            }
        }

        public void AppExit()
        {
            _resolver.CloseService();
            Application.Current.Shutdown();
        }

        public string GetImgPath
        {
            get { return _imgPath; }
            set
            {
                _imgPath = value;
                OnPropertyChanged("GetImgPath");
            }
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

            _peerList = _resolver.GetConnectedPeers();
            OnPropertyChanged("PeerList");

            //_hostList = Tools.getRemoteHosts();
            OnPropertyChanged("HostList");
            
            for(int i=0;i< _peerList.Count;i++)
            {
                _enProduced += _peerList[i].EnProduced;
                _enConsumed += _peerList[i].EnPeak;

                #region checkStatus
                if (_peerList[i].status == PeerStatus.Producer)
                    _numProducers++;

                if (_peerList[i].status == PeerStatus.Consumer)
                    _numConsumers++;
                #endregion
            }

            if (_enConsumed > 0)
                _enThroughput = (_enConsumed / _enProduced) * 100;
            else
                _enThroughput = 0;

            _enTh ="En. Balance: " + Math.Round(_enThroughput, 2) + "%";

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

            for(int i=0;i< _peerList.Count;i++)
            {
                enProd += _peerList[i].EnProduced;
            }

            _enTimeLine.Add(DateTime.Now, enProd);
            OnPropertyChanged("GetPointTimeLine");


            if (_enTimeLine.Count > 500)
                _enTimeLine.Clear();
        }

        private void clockBar_Tick(object sender, EventArgs e)
        {
            i++;

            int ora = i / 3600;
            int minuto = i / 60;
            int secondo = i % 60;

            _ora = ora.ToString("00");
            _minuto = minuto.ToString("00");
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

            _startIsEnabled = false;
            _startColour = "Gray";
            
            _resolverName = _resolver.name;
            _resolverStatus = "Online...";
            _getTimeVisibility = Visibility.Visible;

            _imgPath = @"/WPF_Resolver;component/img/resolver_ok.png";

            OnPropertyChanged("GetImgPath");
            OnPropertyChanged("GetResolverName");         
            OnPropertyChanged("GetResolverStatus");
            OnPropertyChanged("GetResolverIP");
            OnPropertyChanged("GetTimeVisibility");
            OnPropertyChanged("GetIsEnabledStatus");
            OnPropertyChanged("GetStartColour");
        }

        private bool canDo
        {
            get { return true; }
        }
    }
}