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
    class StartPeerViewModel : ViewModelBase
    {
        #region Attributes
        
        private string _name;
        private string _address;
        private string _admin;
        private string _startButton;
        private string _imgPath;
        private string _startButtonIconPath;
        private string _peerStatus;

        private bool _formEnabled;
        private bool _isStartable = true;

        private float _enPeak;
        private float _price;
        private float _enProduced;

        private EnergyType _enType;
        private PeerStatus _status;

        #endregion

        #region Objects
        private Building house;
        private List<ErrorMap> _errorMessages = new List<ErrorMap>();
        #endregion

        #region DelegateCommands
        public DelegateCommand StartPeer { get; set; }
        public DelegateCommand SetProducer { get; set; }
        public DelegateCommand Exit { get; set; }
        public DelegateCommand ViewLog { get; set; }
        #endregion

        public StartPeerViewModel()
        {
            SetErrorMessages();

            _imgPath = @"/WPF_StartPeer;component/img/offline.png";
            _startButtonIconPath = @"/WPF_StartPeer;component/img/disconnected.png";
            _peerStatus = "Offline...";

            _formEnabled = true;
            OnPropertyChanged("FormEnabled");

            _startButton = "Connect";

            _status = PeerStatus.Consumer;

            this.StartPeer = new DelegateCommand((o) => this.Start(), o => this.canDo);
            this.SetProducer = new DelegateCommand((o) => this.Producer(), o => this.canDo);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canDo);
            this.ViewLog = new DelegateCommand((o) => this.Log(), o => this.canDo);
        }

        public string StartButton
        {
            get { return _startButton; }
            set
            {
                _startButton = value;
                OnPropertyChanged("StartButton");
            }
        }

        public bool FormEnabled
        {
            get { return _formEnabled; }
            set
            {
                _formEnabled = value;
                OnPropertyChanged("FormEnabled");
            }
        }

        public void Start()
        {
            CreateBuilding();
        }

        public void Producer()
        {
            if (_status == PeerStatus.Consumer)
                _status = PeerStatus.Producer;
            else
            {
                _status = PeerStatus.Consumer;
                _enType = EnergyType.None;
                _price = 0;

                OnPropertyChanged("EnType");
                OnPropertyChanged("Price");
            }
        }

        public void Log()
        {
            View.LogView LogWindow = new View.LogView();
            LogWindow.ShowDialog();
        }

        private bool canDo
        {
            get { return true; }
        }

        public string Nome
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Nome");
            }
        }
         
        public float EnProduced
        {
            get { return _enProduced; }
            set 
            {
                _enProduced = value;
                OnPropertyChanged("EnProduced");
            }
        }

        public EnergyType EnType
        {
            get { return _enType; }
            set
            {
                _enType = value;
                OnPropertyChanged("Energia");
            }
        }

        public float EnPeak
        {
            get { return _enPeak; }
            set
            {
                _enPeak = value;
                OnPropertyChanged("EnPeak");
            }
        }

        public float Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged("Address");
            }
        }
        
        public string Admin
        {
            get { return _admin; }
            set
            {
                _admin = value;
                OnPropertyChanged("Admin");
            }
        }

        public string Path
        {
            get { return _imgPath; }
            set
            {
                _imgPath = value;
                OnPropertyChanged("Path");
            }
        }

        public string StartButtonIconPath
        {
            get { return _startButtonIconPath; }
            set
            {
                _startButtonIconPath = value;
                OnPropertyChanged("StartButtonIconPath");
            }
        }

        public string GetPeerStatus
        {
            get { return _peerStatus; }
            set
            {
                _peerStatus = value;
                OnPropertyChanged("GetPeerStatus");
            }
        }

        public void AppExit()
        {
            Disconnect();
            Application.Current.Shutdown();
        }

        private void CreateBuilding()
        {
            if (_isStartable)
            {
                if (checkFields() == true)
                {
                    if (_status == PeerStatus.Consumer)
                        EnType = EnergyType.None;

                    Connect();
                }
                else
                {                    
                    MessageBox.Show(getErrorMessages(), Nome  + " - Check your input", MessageBoxButton.OK,MessageBoxImage.Exclamation);
                }
            }
            else
            {
                Disconnect();
            }
        }

        public void Connect()
        {
            house = new Building(Nome, _status, EnType, EnProduced, EnPeak, Price, Address, Admin);

            if (house.isConnected == true)
            {
                house.Start();

                _imgPath = @"/WPF_StartPeer;component/img/online.png";
                OnPropertyChanged("Path");

                _peerStatus = "Online...";
                OnPropertyChanged("GetPeerStatus");

                _startButtonIconPath = @"/WPF_StartPeer;component/img/connected.png";
                OnPropertyChanged("StartButtonIconPath");

                _startButton = "Disconnect";
                OnPropertyChanged("StartButton");

                _isStartable = false;

                _formEnabled = false;
                OnPropertyChanged("FormEnabled");
            }
            else
            {
                if (MessageBox.Show("Unable to contact the Resolver Service. Retry?",
                    "Connection Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    house.Start();
                }
            }
        }

        public void Disconnect()
        {
            _isStartable = true;

            if(house != null)
                house.StopEnergyProduction();

            _startButtonIconPath = @"/WPF_StartPeer;component/img/disconnected.png";
            OnPropertyChanged("StartButtonIconPath");

            _imgPath = @"/WPF_StartPeer;component/img/offline.png";
            OnPropertyChanged("Path");

            _peerStatus = "Offline...";
            OnPropertyChanged("GetPeerStatus");
            
            _startButton = "Connect";
            OnPropertyChanged("StartButton");

            _formEnabled = true;
            OnPropertyChanged("FormEnabled");
        }

        private bool checkFields()
        {
            bool bRet = true;

            for (int i = 0; i < _errorMessages.Count; i++)
                _errorMessages[i].nCheck = 0;

            if ((Nome.Trim()).Length == 0)
            {
                _errorMessages[0].nCheck = 1;
                bRet = false;
            }
            if (_status == PeerStatus.Producer && (EnType == EnergyType.None))
            {
                _errorMessages[1].nCheck = 1;
                bRet = false;
            }
            if (EnPeak == 0)
            {
                _errorMessages[2].nCheck = 1;
                bRet = false;
            }
            if (_status == PeerStatus.Producer && Price == 0)
            {
                _errorMessages[3].nCheck = 1;
                bRet = false;
            }
            if (_status == PeerStatus.Producer && EnProduced == 0)
            {
                _errorMessages[4].nCheck = 1;
                bRet = false;
            }
            if (Address == null || (Address.Trim()).Length == 0)
            {
                _errorMessages[5].nCheck = 1;
                bRet = false;
            }
            if (Admin == null || (Admin.Trim()).Length == 0)
            {
                _errorMessages[6].nCheck = 1;
                bRet = false;
            }

            return bRet;
        }

        private void SetErrorMessages()
        { 
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid Name.")); //0
            _errorMessages.Add(new ErrorMap(0, @"Fill the 'Energy Type' field." )); //1
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid value for 'Energy Peak'.")); //2
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid value for 'Price'.")); //3
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid value for 'Energy Produced'.")); //4
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid Address.")); //5
            _errorMessages.Add(new ErrorMap(0, @"Insert a valid value for 'Admin'.")); //6
        }

        private string getErrorMessages()
        {
            string sRet = "";

            foreach (var m in _errorMessages)
            {
                if (m.nCheck > 0)
                    sRet += m.ErrorMessage + "\n";
            }

            return sRet;
        }

        private class ErrorMap
        {
            public int nCheck { get; set; }
            public string ErrorMessage { get; set; }

            public ErrorMap(int n, string s)
            {
                this.nCheck = n;
                this.ErrorMessage = s;
            }            
        }      
    }
}
