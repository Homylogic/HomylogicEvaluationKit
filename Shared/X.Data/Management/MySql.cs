/* MANAGEMENT FOR SQLITE SERVER
 *
 * Oprava MySQL keď nenačíta používateľov - shell://  mysqlcheck -c  -u root -p --all-databases
 * 
 * Rôzne poomcné funkcie pre prácu s databázov MySQLlite.
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace X.Data.Management
{
    public static class MySql
    {

        /// <summary>
        /// Vytvorí novú prázdnu databázu, ak neexistuje.
        /// </summary>
        /// <param name="dbName">Názov databázy.</param>
        public static void CreateDatabase(string server, string user, string password, string dbName)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder { Server = server, UserID = user, Password = password };
            using var connection = new MySqlConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbName}";
            cmd.ExecuteNonQuery();
            connection.Close();
        }



    }
}
