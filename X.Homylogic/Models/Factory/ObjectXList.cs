/* HOMYLOGIC OBJECT X LIST
 * 
 * Obsahuje zoznam načítaných objektov.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;

namespace X.Homylogic.Models.Factory
{
    public abstract class ObjectXList : X.Data.Factory.DataList
    {

        #region --- DATA LIST ---

        /// <summary>
        /// Nastavit pripojenie k DB tohoto programu.
        /// </summary>
        public override DBClient DBClient => Body.Database.DBClient;

        #endregion

    }
}
