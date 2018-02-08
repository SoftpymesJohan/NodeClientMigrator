using System;
using System.IO;
using Ionic.Zip;

namespace NodeClientMigrator.Core
{
    class Directorys
    {
        public void CreateDir(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteArchive(string d)
        {
            File.Delete(d);
        }

        public void CreateStructure(string pathinit, string dir)
        {
            try
            {
                // Determinar si existe la carpeta Inicial.
                if (!ExistDirectory(pathinit))
                {
                    CreateDir(pathinit);
                    foreach (string d in dir.Split('~'))
                        CreateDir(pathinit + "\\" + d);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        public string Decompress(string filename, string path)
        {
            //string dire = filename.Split('\\')[filename.Split('\\').Length - 1];
            string dire = Path.GetFileName(filename).Replace(".zip", "");
            //string pathdb = path + "\\" + dire.Substring(0, dire.Length - 4);
            string pathdb = path + "\\" + dire;
            if (!ExistDirectory(pathdb))
            {
                CreateDir(pathdb);
                File.Move(filename, pathdb +"\\"+ dire + ".zip");
                using (ZipFile file = new ZipFile(pathdb + "\\" + dire + ".zip"))
                {
                    //Se extrae los archivos del .zip
                    file.ExtractAll(pathdb);
                    //Se guarda el documento en la carpeta creada anteriormente
                    file.Save();
                }
            }
            return pathdb;
        }

        public bool ExistDirectory(string install)
        {
            return Directory.Exists(install);
        }


        public string[] GetFilesDir(string fi,string typefiles)
        {
            return Directory.GetFiles(fi, typefiles);
        }

        public string[] GetDirectorysDir(string backupbk)
        {
            return Directory.GetDirectories(backupbk);
        }

        public string FileName(string bak, string replaceto,string replacewith)
        {
            return Path.GetFileName(bak).Replace(replaceto, replacewith);
        }

    }
}
