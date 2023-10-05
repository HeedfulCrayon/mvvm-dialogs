﻿namespace MvvmDialogs.Views
{
    /// <summary>
    /// Key for the View
    /// </summary>
    public struct ViewKey
    {
        /// <summary>
        /// Thread Id of the view
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// View HashCode
        /// </summary>
        public int ViewHashCode { get; set; }
    }
}