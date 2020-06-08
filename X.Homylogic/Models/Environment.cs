/* APPLICATION EVIRONMENT OBJECTS
 * 
 * Obsahuje inštancie aplikačnách objektov, napr. logovanie, nastavenia, používatelia atď ...
 * 
 * 
 */
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Text;
using X.App;
using X.App.Logs;
using X.Data;

namespace X.Homylogic.Models
{
    public sealed class Environment
    {
        /// <summary>
        /// Zapisovanie do logu.
        /// </summary>
        public LogList Logs { get; private set; }
        /// <summary>
        /// Nastavenia programu.
        /// </summary>
        public Settings Settings { get; private set; }

        public Environment(Models.Database database) 
        {
            this.Logs = new LogList(database.DBClientLogs, database.SyncObjectLogs);
            this.Settings = new Settings(database.DBClient);
        }

        /// <summary>
        /// Načíta zoznam všetkých objektov podľa databázy.
        /// </summary>
        public void Load()
        {
            this.Logs.LoadData(250); // Načítaj maximálne N záznamov, viac nemá význam načítavať (posledných N logov je dostačujúce).
            this.Settings.Load();
        }

    }
}
