/* HISTORY DATA LOGS INTERFACE
 * 
 * Objekty kotré používajú logovanie do histórie údajov.
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace X.Homylogic.Models.Objects.Devices
{
    interface IHistoryDataLogs
    {
        void CreateHistoryTable();
        void DropHistoryTable(long id);
        void WriteHistoryLog();
        void DeleteHistoryLog();
        void SetHistoryData();
    }
}
