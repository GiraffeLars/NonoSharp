using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SQLite;

namespace Picross.Maui.Data
{
    public class Database
    {
        internal const string DatabaseFilename = "PicrossData.db3";

        internal const SQLite.SQLiteOpenFlags Flags = 
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

        internal static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        private SQLiteAsyncConnection? _con = null; 

        private async Task Init()
        {
            if (_con is not null)
                return;

            // Setup connection and create necessary tables
            _con = new SQLiteAsyncConnection(DatabasePath, Flags);
            await _con.CreateTableAsync<Settings>();
        }

        internal async Task<Settings?> GetSettingsAsync() {
            await Init();

            Settings? res = await _con!.Table<Settings>().FirstOrDefaultAsync();
            Debug.WriteLineIf(res == null, "null");
            return res;
        }
    }
}
