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
        private string _imgPath;

        int i = 0;

        #endregion

        #region Objects
        ObservableCollection<TempBuilding> peerList = new ObservableCollection<TempBuilding>();
        System.Windows.Threading.DispatcherTimer temporizzatore;
        private Thread thResolver;
        private Resolver.Resolver r;
        Visibility vi = new Visibility();
        private IPHostEntry _ipHost;
        #endregion

        #region DelegateCommands
        public DelegateCommand StartResolver { get; set; }
        public DelegateCommand Exit { get; set; }
        #endregion

        public ResolverViewModel()
        {
            r = new Resolver.Resolver();

            temporizzatore = new System.Windows.Threading.DispatcherTimer();
            temporizzatore.Interval = new TimeSpan(0, 0, 0, 1);
            temporizzatore.Tick += new EventHandler(Temporizzatore_Tick);

            vi = Visibility.Hidden;
            _ipHost = Dns.GetHostByName(Dns.GetHostName());

            this.StartResolver = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        private void ChooseImgPath()
        {
            foreach (var p in peerList)
            {
                if (p.status.Equals(PeerStatus.Producer))
                {
                    _imgPath = "/img/producer.png";
                    OnPropertyChanged("ImgPath");
                }
                else
                {
                    _imgPath = "/img/consumer.png";
                    OnPropertyChanged("ImgPath");
                }
            }
        }

        public string ImgPath
        {
            get { return _imgPath; }
            set
            {
                _imgPath = value;
                OnPropertyChanged("_imgPath");
            }
        }

        public ObservableCollection<TempBuilding> PeerList
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
            _imgPath = "";

            _resolverName = r.name;
            this.OnPropertyChanged("GetResolverName");

            thResolver = new Thread(r.Connect) { IsBackground = true };
            thResolver.Start();
            thResolver.Join();

            temporizzatore.Start();

            _resolverStatus = "Online...";
            vi = Visibility.Visible;
            

            this.OnPropertyChanged("GetResolverStatus");
            this.OnPropertyChanged("ImgVisibility");
            this.OnPropertyChanged("GetResolverIP");

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

        public Visibility ImgVisibility
        {
            get { return vi; }
            set
            {
                vi = value;
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

        public void Temporizzatore_Tick(object sender, EventArgs e)
        {
            i += 1;

            int ora = i / 3600;
            int minuto = i / 60;
            int secondo = i % 60;

            _ora = ora.ToString("00") + ":";
            _minuto = minuto.ToString("00") + ":";
            _secondo = secondo.ToString("00");

            OnPropertyChanged("GetOra");
            OnPropertyChanged("GetMinuto");
            OnPropertyChanged("GetSecondo");

            peerList = r.GetConnectedPeers();
            OnPropertyChanged("PeerList");
            
            ChooseImgPath();
        }
    }
}