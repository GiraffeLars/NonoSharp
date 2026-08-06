using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui.Data
{
    public class SettingsService
    {
        private readonly Database _db;
        public Settings CurrentSettings { get; private set; } = new();
        
        public SettingsService(Database db) {
            _db = db; 
        }

        public async Task InitializeAsync()
        {
            CurrentSettings = await _db.GetSettingsAsync();
        }
    }
}
