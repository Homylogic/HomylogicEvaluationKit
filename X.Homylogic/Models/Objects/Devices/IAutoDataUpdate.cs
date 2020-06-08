/* AUTO DATA UPDATE INTERFACE
 * 
 * Používa sa pre zariadenia ktoré umožňujú automatické aktualizovanie údajov premenných so vzdialeného zariadenia (po nadviazaní komunikácie).
 * Napr. načítanie údajov meteostanice a nastavení vlastností objektu DataRecord.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace X.Homylogic.Models.Objects.Devices
{
    interface IAutoDataUpdate
    {
        void AutoDataUpdate();
    }
}
