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
        void CreateTableHistory();
        void DropTableHistory(long id);
        void WriteLogHistory();
        void DeleteLogHistory();
        void SetDataHistory();
    }
}
