using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyNotes
{
    public class DBHelper
    {
        public string LocalCacheFolder { get; internal set; }
        private SQLiteConnection sqliteConn = null;
        private string sqliteFile;

        public DBHelper()
        {
            Process exe = Process.GetCurrentProcess();
            string exeFullPath = exe.MainModule.FileName;
            string path = Path.GetDirectoryName(exeFullPath);
            LocalCacheFolder = Path.Combine(path, "DATA");
            Directory.CreateDirectory(LocalCacheFolder);
            sqliteFile = Path.Combine(LocalCacheFolder, "DailyNotes.db");
            Connect();
        }

        /// <summary>
        /// Connect method creates data file if it is not exist and creates the necessary tables
        /// </summary>
        private void Connect()
        {
            string sqliteConnStr = string.Format("Data Source={0};Version=3;", sqliteFile);
            if (!File.Exists(sqliteFile))
            {
                SQLiteConnection.CreateFile(sqliteFile);
            }
            sqliteConn = new SQLiteConnection(string.Format(sqliteConnStr));
            sqliteConn.Open();
            CreateSQLiteTables();
        }

        /// <summary>
        /// Best part of SQLite is "Create If Not Exists" idiom. This way we can create a database at the installation process.
        /// </summary>
        private void CreateSQLiteTables()
        {
            string[] createTables = new string[] {
                "Create Table If Not Exists MasterNote ( ",
                "Id                  INTEGER PRIMARY KEY Autoincrement, ",
                "CreateDate          DateTime DEFAULT (datetime('now','localtime')), ",
                "IsVisible           Bit Default(1), ",
                "Description         TEXT",
                "); ",
                "",
                "Create Table If Not Exists DetailNote ( ",
                "Id                  INTEGER PRIMARY KEY Autoincrement, ",
                "MasterNoteId        INTEGER NOT NULL,",
                "Description         TEXT,",
                "Note                TEXT",
                "IsVisible           Bit Default(1), ",
                "IsCompleted         Bit Default(1), ",
                "StartDate           DateTime DEFAULT (datetime('now','localtime')), ",
                "ScheduledDate       DateTime, ",
                "EndDate             DateTime, ",
                "FOREIGN KEY(MasterNoteId) REFERENCES MasterNote(Id) ",
                "); ",
                "Create Index If Not Exists DetailNote_MasterNoteId On DetailNote(MasterNoteId);",
                "",
            };

            string createScript = string.Join(" ", createTables);
            SQLiteCommand cmd = new SQLiteCommand(createScript, sqliteConn);
            cmd.ExecuteNonQuery();
        }
    }
}
