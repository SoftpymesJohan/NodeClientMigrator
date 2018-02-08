using System;
using System.Linq;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using NodeClientMigrator.Infraestructure;

namespace NodeClientMigrator.Core
{
    class DB
    {
        static string install = null;
        static Directorys direc;
        private MigrationsDBEntities mdb;

        public DB()
        {
            direc = new Directorys();
            mdb = new MigrationsDBEntities();
        }

        private string CreateDataBase(string InstallPathDatabase, string HibernateDatabaseName, string LogFile)
        {
            string tx = "CREATE DATABASE [" + HibernateDatabaseName + "] CONTAINMENT = NONE ON PRIMARY (NAME = N'" + HibernateDatabaseName + "', FILENAME = N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + ".mdf', SIZE = 5120KB, FILEGROWTH = 1024KB ) LOG ON (NAME = N'" + LogFile + "', FILENAME = N'" + InstallPathDatabase + "\\" + HibernateDatabaseName + "_Log.ldf', SIZE = 1024KB, FILEGROWTH = 10 %); ";
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
            return tx;
        }

        private string DeleteDataBase(string HibernateDatabaseName)
        {
            string tx = "USE MASTER; DROP DATABASE [" + HibernateDatabaseName + "]; ";
            return tx;
        }

        private string SizeDataBase(string HibernateDatabaseName, string LogFile)
        {
            string tx = "USE [" + HibernateDatabaseName + "]; DBCC SHRINKFILE (N'" + LogFile + "', 40); ";
            return tx;
        }

        private string GetConnectionValue(string v, string prope)
        {
            return v.Split(';').ToList().Where(p => p.ToUpper().Contains(prope.ToUpper())).First().Split('=')[1];
        }

        private void Log(string logp)
        {
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\BackUp.log", DateTime.Now.ToString() + " - " + logp + Environment.NewLine);
        }

        public void RestoreDatabase(string filename, string path)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                SqlCommand cm = new SqlCommand();
                try
                {
                    var v = ConfigurationManager.ConnectionStrings["MigrationsDBEntities"].ConnectionString.ToString().Split('"')[1];

                    //var v = "Data Source=TECNICO3\\PYMESSQL2014;Initial Catalog=master;User ID=sa;Password=PymesDBPSW1; Connect Timeout=120;";

                    string DataFile = "";
                    string LogFile = "";
                    string FileName = System.IO.Path.GetFileName(filename).Replace(".bak", "");

                    string tx = "";
                    string HibernateInstance = GetConnectionValue(v, "Data Source");
                    string HibernateDatabaseName = GetConnectionValue(v, "Initial Catalog");
                    install = path + "\\" + HibernateInstance.Split('\\')[1];
                    if (!direc.ExistDirectory(install))
                    {
                        direc.CreateDir(install);
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

                    if (!direc.ExistDirectory(InstallPathDatabase))
                        direc.CreateDir(InstallPathDatabase);
                    //filename.Replace(".zip", ".bak") 
                    HibernateDatabaseName = FileName;
                    cm.CommandText = CreateDataBase(InstallPathDatabase, HibernateDatabaseName, LogFile);
                    cm.Connection.Open();
                    cm.ExecuteNonQuery();
                    cm.Connection.Close();

                    tx = "USE[master];";
                    tx += "IF NOT EXISTS(SELECT name FROM sys.filegroups WHERE is_default = 1 AND name = N'PRIMARY') ALTER DATABASE[" + HibernateDatabaseName + "] MODIFY FILEGROUP[PRIMARY] DEFAULT;";
                    cm.CommandText = tx;
                    cm.Connection.Open();
                    cm.ExecuteNonQuery();
                    cm.Connection.Close();

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
                    //
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

                    if (!string.IsNullOrEmpty(path) && direc.ExistDirectory(path))
                    {
                        //Directory.Delete(path, true);
                        path = "";
                    }



                    /*this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Instancias.SelectedIndex = 0;
                    }));*/
                }
            }
        }


        public LogDB IsDB(string name)
        {
            return mdb.LogDB.Where(ldb => ldb.DataBases == name).FirstOrDefault();
        }

        public Process IsDBProcess(int logId)
        {
            return mdb.Process.Where(ldb => ldb.LogId == logId).FirstOrDefault();
        }

        public bool IsDBProcess(string name,bool InitialProcess, bool Processing, bool Finish)
        {
            //return IsDB(name) == null ? false : (IsDB(name).InitialProcess == InitialProcess && IsDB(name).Processing == Processing && IsDB(name).Finish == Finish) ? false : true;
            var i = IsDB(name);
            var p = IsDBProcess(i.LogId);
            return (i != null && p != null && (p.InitialProcess == InitialProcess && p.Processing == Processing && p.Finish == Finish));
        }

        public void AddLogDB(string name, bool InitialProcess)
        {
            LogDB log = new LogDB { DataBases = name };
            mdb.LogDB.Add(log);
            Process pro = new Process { LogId = IsDB(name).LogId, InitialProcess = InitialProcess };
            mdb.Process.Add(pro);
            int changes = mdb.SaveChanges();
        }

        public void UpdateLogDB(string name, bool InitialProcess, bool Processing, bool Finish)
        {
            var d = IsDB(name);//mdb.LogDB.Where(ldb => ldb.DataBases == name && ldb.InitialProcess).FirstOrDefault();
            var p = IsDBProcess(d.LogId);
            if (p.InitialProcess == InitialProcess && p.InitialProcess)
            {
                p.InitialProcess = !InitialProcess;
                p.Processing = true;
                p.DateProcessing = DateTime.Now;
            }
            else if(p.Processing == Processing && p.Processing)
            {
                p.Processing = !Processing;
                p.Finish = true;
                p.DateFinish = DateTime.Now;
            }
            int changes = mdb.SaveChanges();
        }

    }
}
