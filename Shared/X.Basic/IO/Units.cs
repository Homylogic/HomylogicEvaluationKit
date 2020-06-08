using System;
using System.Collections.Generic;
using System.Text;

namespace X.Basic.IO
{
    public static class Units
    {
        /// <summary>
        /// Vráti naformátovanú veľkosť súboru alebo disku, vo vhodnej jednotke napr. 10 MB, 124 kB alebo 1,2 GB
        /// </summary>
        public static string FormatSizeWithUnit(long size)
        {
            if (size < 1024)
                // Bajty
                return size + " B";
            else if (size > 1024 & size < 1024 * 1024)
                // KiloBajky
                return Math.Round(size / (double)1024, 2) + " kB";
            else if (size > 1024 * 1024 & size < 1024 * 1024 * 1024)
                // MegaBajty
                return Math.Round(size / (double)1024 / 1024, 2) + " MB";
            else if (size > 1024 * 1024 * 1024)
                // GigaBajty
                return Math.Round(size / (double)1024 / 1024 / 1024, 2) + " GB";
            else
                // Velkost vrat bytoch
                return size + " B";
        }
    }
}
