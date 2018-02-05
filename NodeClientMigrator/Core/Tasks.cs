using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NodeClientMigrator.Core
{
    class Tasks
    {
        //private static MigrationsDBEntities mdb;
        private static DB db;
        private static Directorys direc;
        private static List<Task> jobs;

        public Tasks()
        {
            //mdb = new MigrationsDBEntities();
            db = new DB();
            direc = new Directorys();
            jobs = new List<Task>();
        }

        #region comentado
        /*private static void AddThread(string backupbk, string pathDataBases)
        {
            Task.Run(() => Metodo(backupbk,pathDataBases)).Wait();
        }
        private static void AddThread2(string backupbk, string backupzip)
        {
            Task.Run(() => Metodo2(backupbk, backupzip)).Wait();
        }*/
        #endregion

        private static void AddThread3(string d, string backupbk,  string pathDataBases)
        {
            Task.Run(() => Metodo3(d,backupbk, pathDataBases)).Wait();
        }

        /*private async static Task Metodo(string backupbk, string pathDataBases)
        {
            foreach (var fi in direc.GetDirectorysDir(backupbk))
            {
                Task job = CallApiAsync(fi, pathDataBases); // se asigna una Task
                jobs.Add(job);
            }
            await Task.WhenAll(jobs); // esperar la ejecución de las llamadas a API
        }

        private async static Task Metodo2(string backupbk, string backupzip)
        {
            foreach (var d in direc.GetFilesDir(backupzip, "*.zip"))
            {
                Task job = CallApiAsync2(d, backupbk); // se asigna una Task
                jobs.Add(job);
            }
            //await Task.WhenAll(jobs); // esperar la ejecución de las llamadas a API
        }*/


        private async static Task Metodo3(string d, string backupbk, string pathDataBases)
        {
            Task job = CallApiAsync2(d, backupbk, pathDataBases); // se asigna una Task
            jobs.Add(job);
            await Task.WhenAll(jobs); // esperar la ejecución de las llamadas a API
        }

        private async static Task CallApiAsync(string fi,string pathDataBases)
        {
            var bak = direc.GetFilesDir(fi, "*.bak").FirstOrDefault();
            if (!String.IsNullOrEmpty(bak))
            {
                string name = direc.FileName(bak,".bak", "");
                //bool isdb = mdb.LogDB.Where(ldb => ldb.DataBases == name && ldb.Processing && !ldb.Finish && !ldb.InitialProcess).FirstOrDefault() == null ? false : true;
                if (!db.IsDBProcess(name,false,true,false))
                {
                    /*var d = db.IsDB(name);//mdb.LogDB.Where(ldb => ldb.DataBases == name && ldb.InitialProcess).FirstOrDefault();
                    d.InitialProcess = false;
                    d.Processing = true;
                    int changes = mdb.SaveChanges();*/
                    db.UpdateLogDB(name,true,false,false);
                    db.RestoreDatabase(bak, pathDataBases);
                    db.UpdateLogDB(name, false, true,false);
                }
                else
                    return;
            }
        }

        private static void ProcessRestaurate(string url, string pathDataBases)
        {
            var bak = direc.GetFilesDir(url, "*.bak").FirstOrDefault();
            if (!String.IsNullOrEmpty(bak))
            {
                string name = direc.FileName(bak, ".bak", "");
                //bool isdb = mdb.LogDB.Where(ldb => ldb.DataBases == name && ldb.Processing && !ldb.Finish && !ldb.InitialProcess).FirstOrDefault() == null ? false : true;
                if (!db.IsDBProcess(name, false, true, false))
                {
                    /*var d = db.IsDB(name);//mdb.LogDB.Where(ldb => ldb.DataBases == name && ldb.InitialProcess).FirstOrDefault();
                    d.InitialProcess = false;
                    d.Processing = true;
                    int changes = mdb.SaveChanges();*/
                    db.UpdateLogDB(name, true, false, false);
                    db.RestoreDatabase(bak, pathDataBases);
                    db.UpdateLogDB(name, false, true, false);
                }
                else
                    return;
            }
        }

        private async static Task CallApiAsync2(string d,string backupbk, string pathDataBases)
        {
            string name = direc.FileName(d, ".zip","");
            string url = "";
            bool isdb = db.IsDB(name) == null ? false : true;
            if (!isdb)
            {
                db.AddLogDB(name, true);
                url = direc.Decompress(d, backupbk);
                direc.DeleteArchive(d);
            } 
            if(!string.IsNullOrEmpty(url))
            {
                ProcessRestaurate(url, pathDataBases);
            }
        }

        private void ProcessStructure(string path,string dir, string pathDataBases)
        {
            try
            {
                string backupzip = (path + "\\" + dir.Split('~')[0]);
                string backupbk = (path + "\\" + dir.Split('~')[1]);
                foreach (var d in direc.GetFilesDir(backupzip, "*.zip"))
                {
                    AddThread3(d,backupbk, pathDataBases);
                    Thread.Sleep(3000);
                }

                //AddThread2(backupbk, backupzip);
                //AddThread(backupbk, pathDataBases);
                //RestauringDB(backupbk);
            }
            catch (Exception)
            {

            }
        }

        public void Structure(string pathinit, string dir,string pathDataBases)
        {
            direc.CreateStructure(pathinit, dir);
            ProcessStructure(pathinit, dir, pathDataBases);
        }

    }
}
