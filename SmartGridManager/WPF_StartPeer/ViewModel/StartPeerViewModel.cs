using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Commons;
using SmartGridManager;
using System.Threading;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using WPF_StartPeer.Command;

namespace WPF_StartPeer.ViewModel
{
    class StartPeerViewModel : TraceListener, INotifyPropertyChanged
    {
        #region Attributes
        
        private string _name;
        private string _address;
        private string _admin;

        private float _enPeak;
        private float _price;
        private float _enProduced;

        private EnergyType _enType;
        private PeerStatus _status;

        #endregion

        #region Objects
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
            this.builder = new StringBuilder();

            this.StartPeer = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.SetProducer = new DelegateCommand((o) => this.Producer(), o => this.canSetProducer);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);
        }

        public void Start()
        {
            string nome;
            if (_status == PeerStatus.Consumer)
            {
                EnType = EnergyType.None;
            }

            house = new Building(Nome, _status, EnType, EnProduced, EnPeak, Price, Address, Admin);

            Trace.AutoFlush = true;
            Trace.Indent();
            nome = "Starting Peer " + Nome + "...";
            Trace.WriteLine(nome);
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

        public void AppExit()
        {
            Application.Current.Shutdown();
        }

        #region Tracing Methods
        public string MyTrace
        {
            get { return this.builder.ToString(); }
        }

        public override void Write(string message)
        {
            this.builder.Append(message);
            this.OnPropertyChanged(new PropertyChangedEventArgs("MyTrace"));
        }

        public override void WriteLine(string message)
        {
            this.builder.AppendLine(message);
            this.OnPropertyChanged(new PropertyChangedEventArgs("MyTrace"));
        }
        #endregion

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
