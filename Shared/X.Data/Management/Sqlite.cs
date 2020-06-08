/* MANAGEMENT FOR SQLITE SERVER
 *
 * SERVER: https://www.sqlite.org/
 * CLINET: https://github.com/pawelsalawa/sqlitestudio
 * 
 * Tutorial: https://www.sqlitetutorial.net/download-install-sqlite/
 * 
 * Rôzne poomcné funkcie pre prácu s databázov SQLlite.
 * 
 */
using System;
using Microsoft.Data.Sqlite;

namespace X.Data.Management
{
    /// <summary>
    /// Rôzne pomocné databázové funkcie.
    /// </summary>
    public static class Sqlite
    {

        /// <summary>
        /// Vráti verziu SQLite servera.
        /// </summary>
        public static string GetVersion()
        {
            using var con = new SqliteConnection("Data Source=:memory:");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT SQLITE_VERSION()"; 
            string version = cmd.ExecuteScalar().ToString();
            con.Close();
            return version;
        }


        /// <summary>
        /// Vytvorí novú prázdnu databázu, ak neexistuje.
        /// </summary>
        /// <param name="path">Umiestnenie alebo názov databázy, napr. './MyDatabase.db' (databáza sa bude nachádzať pri programe).</param>
        public static void CreateDatabase(string dbName) {
            var conString = new SqliteConnectionStringBuilder {DataSource = dbName};
            using var con = new SqliteConnection(conString.ConnectionString);
            con.Open(); // Vytvorí novú DB, ak súbor neexistuje.
            con.Close();
        }

    }

}
