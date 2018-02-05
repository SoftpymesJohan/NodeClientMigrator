using System;
using System.ServiceProcess;
using System.IO;
using NodeClientMigrator.Core;

namespace NodeClientMigrator
{
    public partial class ServiceNodeClientMigrator : ServiceBase
    {
        System.Timers.Timer timer;
        static string dir = null;
        //static string install = null;
        static string pathinitial = null;
        static string pathDataBases = null;
        Tasks tasks = null;


        public ServiceNodeClientMigrator()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.timer = new System.Timers.Timer(10000D);//1000 MILISEGUNDOS O 1 MINUTO
            this.timer.AutoReset = true;
            dir = "Restore~Restoring~Restored";
            tasks = new Tasks();
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.Start();
        }

        protected override void OnStop()
        {
            this.timer.Stop();
            this.timer = null;
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            pathinitial = @"C:\lic\DataBases";
            pathDataBases = @"C:\lic\DataBasesMigration";
            ServiceCrear();
        }

        public void ServiceCrear()
        {
            tasks.Structure(pathinitial,dir,pathDataBases);
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        
        

        static void WriteEventLogEntry(string message)
        {
            // Create an instance of EventLog
            System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog();

            // Check if the event source exists. If not create it.
            if (!System.Diagnostics.EventLog.SourceExists("Generador txt"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Generador FAC.ELE.", "Application");
            }

            // Set the source name for writing log entries.
            eventLog.Source = "Generador txt";

            // Create an event ID to add to the event log
            int eventID = 8;

            // Write an entry to the event log.
            eventLog.WriteEntry(message,
                                System.Diagnostics.EventLogEntryType.Warning,
                                eventID);

            // Close the Event Log
            eventLog.Close();

        }

        private static void Log(string logp)
        {
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\BackUp.log", DateTime.Now.ToString() + " - " + logp + Environment.NewLine);
        }
    }
}
