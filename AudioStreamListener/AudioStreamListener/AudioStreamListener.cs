using ASLCommands;
using CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Messaging;
using System.ServiceProcess;
using System.Text;
using log4net;
using System.Timers;

namespace AudioStreamListener
{
    public partial class AudioStreamListener : ServiceBase
    {
        private ILog log;
        private MessageQueue commandQueue;

        public SerialPort sp;
        public Timer t;
        public MMDeviceEnumerator DevEnum;
        public MMDevice device;

        public bool justEnabled;
        public bool playing;
        public string color;

        public AudioStreamListener(ILog log)
        {
            InitializeComponent();

            this.log = log;

            commandQueue = OpenMessageQueue(ConfigurationManager.AppSettings.Get("CommandQueueName"), false);
            this.commandQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Command) });
            commandQueue.ReceiveCompleted += commandQueue_ReceiveCompleted;

            DevEnum = new MMDeviceEnumerator();

            sp = new SerialPort();
            sp.PortName = ConfigurationManager.AppSettings.Get("DefaultPort");

            color = ConfigurationManager.AppSettings.Get("DefaultColor");

            log.DebugFormat("PortName: {0}", sp.PortName);
            log.DebugFormat("Color: {0}", color);

            t = new Timer(500);
            t.Elapsed += t_Elapsed;
        }

        void commandQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            try
            {
                var command = e.Message.Body as Command;

                log.DebugFormat("Type: {0}, Value: {1}", command.CommandType, command.Value);

                switch (command.CommandType)
                {
                    case ASLCommands.CommandType.Status:
                        HandleStatusCommand(command.Value);
                        break;
                    case ASLCommands.CommandType.Color:
                        HandleColorCommand(command.Value);
                        break;
                    case ASLCommands.CommandType.Port:
                        HandlePortCommand(command.Value);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception: {0}", ex.ToString());
            }
            finally
            {
                commandQueue.BeginReceive();
            }
        }

        protected override void OnStart(string[] args)
        {
            commandQueue.BeginReceive();
            t.Start();
        }

        protected override void OnStop()
        {
            t.Stop();
            SendSerialCommand("o");
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();

            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

            var volume = device.AudioMeterInformation.MasterPeakValue;
            
            log.DebugFormat("Time Elapsed. Volume: {0}", volume);
            
            if (volume > 0.01)
            {
                if (!playing || justEnabled)
                {
                    SendSerialCommand(color);
                }

                playing = true;
            }
            else
            {
                if (playing || justEnabled)
                {
                    SendSerialCommand("o");
                }

                playing = false;
            }

            t.Start();
        }

        #region Handle Commands

        private void HandleStatusCommand(string newStatus)
        {
            log.DebugFormat("Setting Status: {0}", newStatus);

            t.Stop();

            switch (newStatus)
            {
                case "Enabled":
                    SendSerialCommand("o");
                    justEnabled = true;
                    t.Start();
                    break;
                case "Disabled":
                    SendSerialCommand("o");
                    break;
                case "Spoofed":
                    SendSerialCommand(color);
                    break;
            }
        }

        private void HandleColorCommand(string newColor)
        {
            log.DebugFormat("Setting Color: {0}", newColor);
            color = newColor.ToLower()[0].ToString();
            SendSerialCommand(color);
        }

        private void HandlePortCommand(string newPort)
        {
            log.DebugFormat("Setting Port: {0}", newPort);

            sp = new SerialPort();
            sp.PortName = ConfigurationManager.AppSettings.Get("DefaultPort");
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

        private void SendSerialCommand(string value)
        {
            log.DebugFormat("Sending Command: {0}", value);

            try
            {
                sp.Open();
                sp.Write(value);
                log.InfoFormat("Received from device: {0}", sp.ReadLine());
                sp.Close();
            }
            catch (Exception e)
            {
                log.ErrorFormat("Serial Error: {0}", e);
            }
        }

        #endregion
    }
}
