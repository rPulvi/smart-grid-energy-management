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

        private readonly StringBuilder builder;
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

            _startButton = "Connect";

            _status = PeerStatus.Consumer;
            this.builder = new StringBuilder();

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
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));
            }
        }

        public void Start()
        {
            CreateBuilding();
        }

        public void Producer()
        {
            _status = PeerStatus.Producer;
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
                this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));

                _peerStatus = "Online...";
                this.OnPropertyChanged(new PropertyChangedEventArgs("GetPeerStatus"));

                _startButtonIconPath = @"/WPF_StartPeer;component/img/connected.png";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButtonIconPath"));

                _startButton = "Disconnect";
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));

                _isStartable = false;
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
            if(house != null)
                house.StopEnergyProduction();

            _startButtonIconPath = @"/WPF_StartPeer;component/img/disconnected.png";
            this.OnPropertyChanged(new PropertyChangedEventArgs("StartButtonIconPath"));

            _imgPath = @"/WPF_StartPeer;component/img/offline.png";
            this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));

            _peerStatus = "Offline...";
            this.OnPropertyChanged(new PropertyChangedEventArgs("GetPeerStatus"));

            _isStartable = true;

            _startButton = "Connect";
            this.OnPropertyChanged(new PropertyChangedEventArgs("StartButton"));
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
