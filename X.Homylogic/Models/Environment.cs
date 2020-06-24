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
using X.App.Users;
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
        /// Application users.
        /// </summary>
        public Users Users { get; private set; }
        /// <summary>
        /// Nastavenia programu.
        /// </summary>
        public Settings Settings { get; private set; }

        public Environment(Models.Database database) 
        {
            this.Logs = new LogList(database.DBClientLogs);
            this.Users = new Users();
            this.Settings = new Settings(database.DBClient.Clone());
        }

        /// <summary>
        /// Načíta zoznam všetkých objektov podľa databázy (okrem používateľov).
        /// </summary>
        public void Load()
        {
            this.Logs.LoadData(250); // Načítaj maximálne N záznamov, viac nemá význam načítavať (posledných N logov je dostačujúce).
            this.Settings.Load();
        }
        /// <summary>
        /// Loads users only.
        /// </summary>
        public void LoadUsers() 
        {
            this.Users.Load();
        }

    }
}
