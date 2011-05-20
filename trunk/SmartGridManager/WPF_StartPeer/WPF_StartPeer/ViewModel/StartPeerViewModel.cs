using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Commons;
using SmartGridManager;
//using WPF_StartPeer.Command;
using System.Threading;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using WPF_StartPeer.Command;

namespace WPF_StartPeer.ViewModel
{
    public class StartPeerViewModel : TraceListener, INotifyPropertyChanged
    {
        private string nome;
        private EnergyType energia;
        private float enpeak;
        private float price;
        private bool producer;
        string output;
        
        private readonly StringBuilder builder;

        Building house;
        List<Building> buildings = new List<Building>();

        public DelegateCommand StartPeer { get; set; }
        public DelegateCommand SetProducer { get; set; }
        public DelegateCommand Exit { get; set; }

        public List<Building> getBuildings()
        {
            return this.buildings;
        }

        public StartPeerViewModel()
        {
            this.builder = new StringBuilder();

            this.StartPeer = new DelegateCommand((o) => this.Start(), o => this.canStart);
            this.SetProducer = new DelegateCommand((o) => this.Producer(), o => this.canSetProducer);
            this.Exit = new DelegateCommand((o) => this.AppExit(), o => this.canExit);

            //TODO: leggere periodicamente
        }

        public void Start()
        {

            if (!producer)
            {
                Energia = EnergyType.None;
            }

            house = new Building(Nome, Energia, EnPeak, Price);

            buildings.Add(house);
            
            if (producer)
            {
                house.setEnergyLevel(90);
            }

            Trace.AutoFlush = true;
            Trace.Indent();
            Trace.WriteLine("Starting Peer {0} ...", Nome);

            //tmpOutput = "Starting Peer ..." + Nome;
            //File.WriteAllText("output.txt", tmpOutput);

        }

        public void Producer()
        {
            producer = true;
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
            get { return nome; }
            set
            {
                nome = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Nome"));
            }
        }

        public EnergyType Energia
        {
            get { return energia; }
            set
            {
                energia = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Energia"));
            }
        }

        public float EnPeak
        {
            get { return enpeak; }
            set
            {
                enpeak = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("EnPeak"));
            }
        }

        public float Price
        {
            get { return price; }
            set
            {
                price = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Price"));
            }
        }

        public string LeggiFile
        {
            get
            {
                return output = File.ReadAllText("output.txt");
            }
        }

        public void AppExit()
        {
            Application.Current.Shutdown();
        }

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
