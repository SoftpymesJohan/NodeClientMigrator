using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace NodeClientMigrator
{
    public partial class ServiceNodeClientMigrator : ServiceBase
    {
        Timer timer;
        static string dir = null; 

        <

        public ServiceNodeClientMigrator()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.timer = new System.Timers.Timer(1000D);//1000 MILISEGUNDOS O 1 MINUTO
            this.timer.AutoReset = true;
            dir = "Restore~Restoring~Restored";
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
            ServiceCrear();
        }

        public static void ServiceCrear()
        {
            CreateStructure(@"E:\Pymes Migrator");
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        private static void CreateStructure(string pathinit)
        {
            try
            {
                string path = pathinit + "/DataBases";
                // Determinar si existe la carpeta Inicial.
                if (!Directory.Exists(path))
                {
                    CreateDir(path);
                    foreach(string d in dir.Split('~'))
                        CreateDir(path + "/" + d);
                }
                else
                {
                    foreach (string d in dir.Split('~'))
                    {
                        string backupzip = (path + "/" + d);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        private static void CreateDir(string path)
        {
            DirectoryInfo di = Directory.CreateDirectory(path);
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
    }
}
