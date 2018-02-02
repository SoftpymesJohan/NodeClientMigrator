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
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
//using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Ionic.Zip;
using Microsoft.Win32;
using System.IO.Compression;

namespace NodeClientMigrator
{
    public partial class ServiceNodeClientMigrator : ServiceBase
    {
        Timer timer;
        static string dir = null;
        static string install = null;
        static string pathinitial = null;
        static string pathDataBases = null;


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
            pathinitial = @"D:\lic\DataBases";
            pathDataBases = @"D:\lic\DataBasesMigration";
            ServiceCrear();
            
        }

        public static void ServiceCrear()
        {
            CreateStructure(pathinitial);
            ProcessStructure(pathinitial);
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        private static void CreateStructure(string pathinit)
        {
            try
            {
                // Determinar si existe la carpeta Inicial.
                if (!Directory.Exists(pathinit))
                {
                    CreateDir(pathinit);
                    foreach(string d in dir.Split('~'))
                        CreateDir(pathinit + "\\" + d);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        private static void ProcessStructure(string path)
        {
            try
            {
                string backupzip = (path + "\\" + dir.Split('~')[0]);
                string backupbk = (path + "\\" + dir.Split('~')[1]);
                var dirs = Directory.GetFiles(backupzip, "*.zip");
                foreach (var d in dirs)
                {
                    Decompress(d, backupbk);
                    File.Delete(d);
                    //RestoreDatabase(d, pathDataBases);
                }
                DirectoryInfo di = new DirectoryInfo(backupbk+"\\");
                foreach (var fi in Directory.GetDirectories(backupbk))
                {
                    var bak = Directory.GetFiles(fi, "*.bak").FirstOrDefault();
                    if (!String.IsNullOrEmpty(bak))
                    {
                        RestoreDatabase(bak,pathDataBases);
                    }
                    Directory.Delete(fi, true);
                }
            }
            catch (Exception)
            {
                
            }
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

        private static void Log(string logp)
        {
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\BackUp.log", DateTime.Now.ToString() + " - " + logp + Environment.NewLine);
        }

        private static void Decompress(string filename,string path)
        {
            //string dire = filename.Split('\\')[filename.Split('\\').Length - 1];
            string dire = System.IO.Path.GetFileName(filename).Replace(".zip", "");
            //string pathdb = path + "\\" + dire.Substring(0, dire.Length - 4);
            string pathdb = path + "\\" + dire;
            if (!Directory.Exists(pathdb))
            {
                CreateDir(pathdb);
                using (ZipFile file = new ZipFile(filename))
                {
                    //Se extrae los archivos del .zip
                    file.ExtractAll(pathdb);
                    //Se guarda el documento en la carpeta creada anteriormente
                    file.Save();
                }
            }
        }

        

        private static void RestoreDatabase(string filename,string path)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                SqlCommand cm = new SqlCommand();
                try
                {

                    var v = "Data Source=TECNICO3\\PYMESSQL2014;Initial Catalog=master;User ID=sa;Password=PymesDBPSW1; Connect Timeout=120;";

                    string DataFile = "";
                    string LogFile = "";
                    string FileName = System.IO.Path.GetFileName(filename).Replace(".bak", "");

                    string tx = "";
                    string HibernateInstance = GetConnectionValue(v, "Data Source");
                    string HibernateDatabaseName = GetConnectionValue(v, "Initial Catalog");
                    install = path + "\\" + HibernateInstance.Split('\\')[1];
                    if (!Directory.Exists(install))
                    {
                        CreateDir(install);
                    }
                    string sc = v.Replace(HibernateDatabaseName, "master");

                    SqlConnection conn = new SqlConnection(v.Replace(HibernateDatabaseName, "master"));
                    string[] tempfilename = filename.Split(' ');
                    Log(v.ToString());
                    tx = "RESTORE FILELISTONLY FROM DISK = '" + filename.Replace(".zip", ".bak") + "'";
                    cm.Connection = conn;
                    cm.CommandTimeout = 3600;
                    cm.CommandText = tx;
                    cm.Connection.Open();

                    SqlDataReader rd = cm.ExecuteReader();
                    while (rd.Read())
                    {
                        if (rd["Type"].ToString() == "D")
                        {
                            DataFile = rd["LogicalName"].ToString();
                        }
                        else if (rd["Type"].ToString() == "L")
                        {
                            LogFile = rd["LogicalName"].ToString();
                        }
                    }
                    rd.Close();
                    cm.Connection.Close();
                    Log("Base de datos " + DataFile + " Y Log " + LogFile);

                    string InstallPathDatabase = install + "\\" + FileName /*+ instanceSelection.Split('\\')[1] + "\\"*/;

                    if (!Directory.Exists(InstallPathDatabase))
                        Directory.CreateDirectory(InstallPathDatabase);
                    //filename.Replace(".zip", ".bak") 
                    tx = " DATABASE [" + HibernateDatabaseName + "] CONTAINMENT = NONE ON PRIMARY (NAME = N'" + DataFile + "', FILENAME = N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + ".mdf', SIZE = 5120KB, FILEGROWTH = 1024KB ) LOG ON (NAME = N'" + LogFile + "', FILENAME = N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + "_Log.ldf', SIZE = 1024KB, FILEGROWTH = 10 %); ";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET COMPATIBILITY_LEVEL = 110;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET ANSI_NULL_DEFAULT OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET ANSI_NULLS OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET ANSI_PADDING OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET ANSI_WARNINGS OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET ARITHABORT OFF;"; 
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET AUTO_CLOSE OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET AUTO_CREATE_STATISTICS ON;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET AUTO_SHRINK OFF; ";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET AUTO_UPDATE_STATISTICS ON;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET CURSOR_CLOSE_ON_COMMIT OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET CURSOR_DEFAULT  GLOBAL;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET CONCAT_NULL_YIELDS_NULL OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET NUMERIC_ROUNDABORT OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET QUOTED_IDENTIFIER OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET RECURSIVE_TRIGGERS OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET DISABLE_BROKER;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET AUTO_UPDATE_STATISTICS_ASYNC OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET DATE_CORRELATION_OPTIMIZATION OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET PARAMETERIZATION SIMPLE;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET READ_COMMITTED_SNAPSHOT OFF;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET READ_WRITE;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET RECOVERY SIMPLE;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET MULTI_USER;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET PAGE_VERIFY CHECKSUM;";
                    tx += "ALTER DATABASE[" + HibernateDatabaseName + "] SET TARGET_RECOVERY_TIME = 0 SECONDS;";
                    tx += "USE[" + HibernateDatabaseName + "];";
                    tx += "IF NOT EXISTS(SELECT name FROM sys.filegroups WHERE is_default = 1 AND name = N'PRIMARY') ALTER DATABASE[" + HibernateDatabaseName + "] MODIFY FILEGROUP[PRIMARY] DEFAULT;";
                    cm.CommandText = tx;
                    cm.Connection.Open();
                    cm.ExecuteNonQuery();
                    cm.Connection.Close();

                    cm.CommandText = "ALTER DATABASE [" + HibernateDatabaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    cm.Connection.Open();
                    cm.ExecuteNonQuery();
                    cm.Connection.Close();

                    HibernateDatabaseName = FileName;
                    //Se utiliza el Alter para modificar la base para SINGLE_USER controlando que no se use por otros procesos
                    tx = "ALTER DATABASE [" + HibernateDatabaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ";
                    //if (DataFile.Equals("SiesaPYMES"))
                    // {
                    //tx += DeleteDataBase(HibernateDatabaseName);
                    //}
                    //Se restaura la Base de datos en modo SINGLE_USER
                    //tx += "RESTORE DATABASE [" + HibernateDatabaseName + "] FROM DISK = '" + filename.Replace(".zip", ".bak") + "' WITH FILE = 1, ";
                    tx += "RESTORE DATABASE [" + HibernateDatabaseName + "] FROM DISK = '" + filename.Replace(".zip", ".bak") + "' WITH FILE = 1, ";
                    tx += "MOVE N'" + DataFile + "' TO N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + ".mdf',";
                    tx += " MOVE N'" + LogFile + "' TO N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + "_Log.ldf', NOUNLOAD, REPLACE, STATS = 10; ";
                    //tx += SizeDataBase(HibernateDatabaseName, LogFile);
                    Log(tx);
                    cm.CommandText = tx;
                    cm.Connection.Open();

                    int i = cm.ExecuteNonQuery();

                    Log(tx + " Result " + i);
                    tx = "1";
                    cm.Connection.Close();
                    //Se vuelve a modificar la base de datos para que quede multi usuario
                    cm.CommandText = "ALTER DATABASE [" + HibernateDatabaseName + "] SET MULTI_USER";
                    cm.Connection.Open();
                    cm.ExecuteNonQuery();
                    //cm.Connection.Close();
                    
                }
                catch (Exception ex)
                {
                    Log("Se ha producido un error al sacar un BackUp de la instancia." + ex.InnerException.ToString());
                    /*if (ex.ToString().Contains("The database was backed up on a server running version 12.00.2000"))
                    {
                        MessageBox.Show("No se ha podido restaurar la base de datos. \n\n El archivo backup de la base de datos, \n\n fue realizado con una version posterior " + ex.InnerException.ToString(), "Backup Pymes + ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("No se ha podido restaurar la base de datos. \n\n El siguiente es el detalle del error " + ex.ToString(), "Backup Pymes + ", MessageBoxButton.OK, MessageBoxImage.Error);
                        //this.Dispatcher.BeginInvoke(new Action(() => { Log("Se ha producido un error al sacar un BackUp de la instancia." + ex.InnerException.ToString()); }));
                        Log("Se ha producido un error al sacar un BackUp de la instancia." + ex.InnerException.ToString());
                        //pg1.IsIndeterminate = false; SetButtons(true);
                    }*/
                }

                finally
                {
                    cm.Connection.Close();
                    cm.Dispose();

                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        path = "";
                    }



                    /*this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Instancias.SelectedIndex = 0;
                    }));*/
                }
            }
        }


        private static string DeleteDataBase(string HibernateDatabaseName)
        {
            string tx = "USE MASTER; DROP DATABASE [" + HibernateDatabaseName + "]; ";
            return tx;
        }

        private static string SizeDataBase(string HibernateDatabaseName, string LogFile)
        {
            string tx = "USE [" + HibernateDatabaseName + "]; DBCC SHRINKFILE (N'" + LogFile + "', 40); ";
            return tx;
        }

        private static string GetConnectionValue(string v, string prope)
        {
            return v.Split(';').ToList().Where(p => p.Contains(prope)).First().Split('=')[1];
        }
    }
}
