using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Class containing options for <see cref="NonogramAPI"/>
    /// </summary>
    public class NonogramOptions
    {
        /// <summary>
        /// Whether blank cells should be crossed out when the hints of a line are completed.
        /// <c>true</c> by default.
        /// </summary>
        public bool EnableAutoCross { get; set; } = true;

        /// <summary>
        /// Whether to automatically correct cells that are filled or crossed incorrectly according to the puzzle's solution.
        /// <c>false</c> by default
        /// </summary>
        public bool EnableAutoCorrection { get; set; } = false;

        /// <summary>
        /// Initialises a NonogramOptions instance with default values
        /// </summary>
        public NonogramOptions()
        { }
    }
}
