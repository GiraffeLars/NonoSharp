using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui
{
    internal class Theme
    {
        private static bool IsDarkMode()
        {
            AppTheme? systemTheme = Application.Current?.RequestedTheme;
            return systemTheme == AppTheme.Dark;
        }

        public static Color FilledCell => IsDarkMode() ? Colors.White : Colors.Black;
        public static Color SolvedCell => Colors.Green;
        public static Color GridLine => IsDarkMode() ? Colors.LightGray : Colors.Gray;
        public static Color IncompleteHint => IsDarkMode() ? Colors.White : Colors.Black;
        public static Color CompletedHint => IsDarkMode() ? Colors.LightGray : Colors.Gray;
        public static Color Background => IsDarkMode() ? Colors.Black : Colors.White;
    }
}
