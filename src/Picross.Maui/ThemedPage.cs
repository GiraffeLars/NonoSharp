using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui
{
    /// <summary>
    /// A <c>ContentPage</c> that automatically changes themes when the system's theme changes
    /// </summary>
    public partial class ThemedPage : ContentPage
    {
        public ThemedPage() : base() 
        {
            // Set Background color (in case of system mismatch) and add event handling when system theme changes
            BackgroundColor = Theme.BackgroundColor;
            Application.Current!.RequestedThemeChanged += (s, a) =>
            {
                BackgroundColor = Theme.BackgroundColor;
            };
        }
    }
}
