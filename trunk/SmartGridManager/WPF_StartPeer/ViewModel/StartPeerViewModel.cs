using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Commons;
using SmartGridManager;
using System.Threading;
using System.IO;
using System.Windows;
using System.ComponentModel;
using WPF_StartPeer.Command;

namespace WPF_StartPeer.ViewModel
{
    class StartPeerViewModel : INotifyPropertyChanged
    {
        #region Attributes
        
        private string _name;
        private string _address;
        private string _admin;
        private string _startButton;
        private string _imgPath;
        private string _startButtonIconPath;
        private string _peerStatus;

        private bool _isStartable = true;

        private float _enPeak;
        private float _price;
        private float _enProduced;

        private EnergyType _enType;
        private PeerStatus _status;

        #endregion

        #region Objects
        private BackgroundWorker bw = new BackgroundWorker();
        private readonly StringBuilder builder;
        Building house;
        #endregion

        #region DelegateCommands
        public DelegateCommand StartPeer { get; set; }
        public DelegateCommand SetProducer { get; set; }
        public DelegateCommand Exit { get; set; }
        #endregion

        public StartPeerViewModel()
        {
            _imgPath = @"/WPF_StartPeer;component/img/offline.png";
            _startButtonIconPath = @"/WPF_StartPeer;component/img/disconnected.png";
            _peerStatus = "Offline...";

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);

            _startButton = "Start";

            _status = PeerStatus.Consumer;
            this.builder = new StringBuilder();

            this.StartPeer = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.SetProducer = new DelegateCommand((o) => this.Producer(), o => this.canSetProducer);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        public string StartButton
        {
            get { return _startButton; }
            set
            {
                _startButton = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));
            }
        }

        public void Start()
        {
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        public void Producer()
        {
            _status = PeerStatus.Producer;
        }

        private bool canStart
        {
            get { return true; }
        }

        private bool canSetProducer
        {
            get { return true; }
        }

        private bool canExit
        {
            get { return true; }
        }

        public string Nome
        {
            get { return _name; }
            set
            {
                _name = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Nome"));
            }
        }
         
        public float EnProduced
        {
            get { return _enProduced; }
            set 
            {
                _enProduced = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("EnProduced"));
            }
        }

        public EnergyType EnType
        {
            get { return _enType; }
            set
            {
                _enType = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Energia"));
            }
        }

        public float EnPeak
        {
            get { return _enPeak; }
            set
            {
                _enPeak = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("EnPeak"));
            }
        }

        public float Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Price"));
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Address"));
            }
        }
        
        public string Admin
        {
            get { return _admin; }
            set
            {
                _admin = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Admin"));
            }
        }

        public string Path
        {
            get { return _imgPath; }
            set
            {
                _imgPath = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));
            }
        }

        public string StartButtonIconPath
        {
            get { return _startButtonIconPath; }
            set
            {
                _startButtonIconPath = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButtonIconPath"));
            }
        }

        public string GetPeerStatus
        {
            get { return _peerStatus; }
            set
            {
                _peerStatus = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("GetPeerStatus"));
            }
        }

        public void AppExit()
        {
            Application.Current.Shutdown();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isStartable)
            {
                if (_status == PeerStatus.Consumer)
                {
                    EnType = EnergyType.None;
                }

                house = new Building(Nome, _status, EnType, EnProduced, EnPeak, Price, Address, Admin);

                _imgPath = @"/WPF_StartPeer;component/img/online.png";
                this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));

                _peerStatus = "Online...";
                this.OnPropertyChanged(new PropertyChangedEventArgs("GetPeerStatus"));

                _startButtonIconPath = @"/WPF_StartPeer;component/img/connected.png";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButtonIconPath"));

                _startButton = "Stop";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));

                _isStartable = false;
            }
            else
            {
                house.StopEnergyProduction();

                _startButtonIconPath = @"/WPF_StartPeer;component/img/disconnected.png";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButtonIconPath"));

                _imgPath = @"/WPF_StartPeer;component/img/offline.png";
                this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));

                _peerStatus = "Offline...";
                this.OnPropertyChanged(new PropertyChangedEventArgs("GetPeerStatus"));

                _isStartable = true;

                _startButton = "Start";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
