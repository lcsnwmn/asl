using ASLCommands;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASLConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageQueue commandQueue;

        public List<AslStatus> Statuses { get; set; }
        public AslStatus SelectedStatus { get; set; }

        public List<AslColor> Colors { get; set; }
        public AslColor SelectedColor { get; set; }

        public List<string> Ports { get; set; }
        public string SelectedPort { get; set; }

        public ICommand SetStatus { get; private set; }
        public ICommand SetColor { get; private set; }
        public ICommand SetPort { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.commandQueue = OpenMessageQueue(ConfigurationManager.AppSettings.Get("CommandQueueName"), false);
            this.commandQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Command) });

            LoadStatuses();
            LoadColors();
            LoadPorts();

            InitializeCommands();
        }

        #region Initialization

        private void LoadStatuses()
        {
            Statuses = Enum.GetValues(typeof(AslStatus)).Cast<AslStatus>().ToList();
        }

        private void LoadColors()
        {
            Colors = Enum.GetValues(typeof(AslColor)).Cast<AslColor>().ToList();
        }

        private void LoadPorts()
        {
            Ports = SerialPort.GetPortNames().ToList();
        }

        private void InitializeCommands()
        {
            SetStatus = new DelegateCommand(SetStatus_Executed);
            SetColor = new DelegateCommand(SetColor_Executed);
            SetPort = new DelegateCommand(SetPort_Executed);
        } 

        #endregion

        #region Commands

        private void SetStatus_Executed()
        {
            var command = new Command
            {
                CommandType = CommandType.Status,
                Value = SelectedStatus.ToString()
            };

            commandQueue.Send(new Message(command), string.Format("Change Status to {0}", SelectedStatus.ToString()));
        }

        private void SetColor_Executed()
        {
            var command = new Command
            {
                CommandType = CommandType.Color,
                Value = SelectedColor.ToString()
            };

            commandQueue.Send(new Message(command), string.Format("Change Color to {0}", SelectedColor.ToString()));
        }

        private void SetPort_Executed()
        {
            var command = new Command
            {
                CommandType = CommandType.Port,
                Value = SelectedPort
            };

            commandQueue.Send(new Message(command), string.Format("Change Port to {0}", SelectedPort));
        } 

        #endregion

        #region Helpers

        private MessageQueue OpenMessageQueue(string queueName, bool transactional)
        {
            MessageQueue queue = null;

            if ((queueName.StartsWith(".") == true) && (MessageQueue.Exists(queueName) == false))
            {
                queue = MessageQueue.Create(queueName, transactional);
                queue.SetPermissions("Everyone", MessageQueueAccessRights.WriteMessage | MessageQueueAccessRights.ReceiveMessage);
            }
            else
            {
                queue = new MessageQueue(queueName);
            }

            return queue;
        }

        #endregion

    }
}
