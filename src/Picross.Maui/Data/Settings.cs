using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui.Data
{
    internal class Settings
    {
        [PrimaryKey]
        public int ID { get; set; } = 1; // Settings is a single-row database

        public string Theme { get; set; } = "System"; // Expected to be System, Light, Dark
    }
}
