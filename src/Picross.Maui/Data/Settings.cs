using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui.Data
{
    public class Settings : ICloneable
    {

        [PrimaryKey]
        public int ID { get; set; } = 1; // Settings is a single-row database, set property must be present otherwise an error is thrown when updating

        public AppTheme Theme { get; set; } = AppTheme.Unspecified;
     

        /// <summary>
        /// Returns a shallow clone, specifically one created by <see cref="object.MemberwiseClone"/>.
        /// </summary>
        /// <returns>Settings instance as described above</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
