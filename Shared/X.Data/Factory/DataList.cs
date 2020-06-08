/* DATA LIST
 * 
 * Obsahuje zoznam objektov DataRecord, pričom ich prepája s databázov.
 * 
 * 
 */
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace X.Data.Factory
{
    public abstract class DataList
    {
        /// <summary>
        /// Získanie pripojenia k DB z horného classu (ktorý dedí tento class).
        /// </summary>
        public abstract DBClient DBClient { get; }
        /// <summary>
        /// Vráti nový inicializovný objekt DataRecord, počas načítania z databázy je možné upresniť inicializáciu presného typu classu podľa databázy.
        /// </summary>
        /// <param name="dbReader">Používa sa pre vytvorenie inštancie rôznych typov objektu dataRecord podľa databázy, ešte pred nastavením údajov vlastností.</param>
        public abstract DataRecord GetInitializedDataRecord(DBReader dbReader = null);

        protected BindingList<DataRecord> _list = new BindingList<DataRecord>();
        /// <summary>
        /// Obsahuje načítaný zoznam objektov.
        /// </summary>
        public BindingList<DataRecord> List => _list; 
        /// <summary>
        /// Používa sa pre synchronizáciu objektu List z hlavným vláknom aplikácie, aby bolo možné pridávať objekty DataRecord do BindingListu aj z vlákna.
        /// Problém vzníká keď je BindingList nabindovaný (napr. na DataGrind) pričom pridávanie záznamov prebieha z vlákna.
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; set; }
        /// <summary>
        /// SQL WHERE (bez WHERE, napr. ID = 1) podmienka ktorá sa použije pre načítanie a vymazanie záznamov.
        /// </summary>
        public string FilterCondition { get; set; }
        /// <summary>
        /// Či boli už údaje načítané.
        /// </summary>
        public bool IsDataLoaded { get; protected set; }

        /// <summary>
        /// Načítanie záznamov z databázy a vytvorenie objektov DataRecord do Listu.
        /// </summary>
        /// <param name="sorting">Triedenie záznamov, zadáva sa sql formát, napr. Name DESC, ID ASC.</param>
        /// <param name="recordsLimit">Maxinálny počet načítaných záznamov.</param>
        public void LoadData(string sorting = null, int recordsLimit = 0) 
        {
            // Vymazanie už načítaných objektov.
            ///
            // ****** TODO: DOROBIŤ ZATVÁRANIE OBJEKTOV ---- 
            _list.Clear();

            // Vytvor dočasne pomocný objekt DataRecord, napr. pre zistenie názvu DB tabuľky.     
            DataRecord dataRecord = this.GetInitializedDataRecord();

            // Pripojenie k DB, spustenie SQL a prechádzanie načítaným zoznamom.
            this.DBClient.Open();

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("SELECT * FROM {0}", dataRecord.TableName);
            if (!string.IsNullOrEmpty(this.FilterCondition))
                sql.AppendFormat(" WHERE {0}", this.FilterCondition);
            if (!string.IsNullOrEmpty(sorting))
                sql.AppendFormat(" ORDER BY {0}", sorting);

            using DBReader dbReader = this.DBClient.ExecuteReader(sql.ToString());
            int count = 0;

            while (dbReader.Read())
            {
                // Vytvoriť nový objekt DataRecord, nastaviť jeho hodnoty podľa databázy a pridať do List-u.
                dataRecord = this.GetInitializedDataRecord(dbReader);
                dataRecord.ParentDataList = this;
                List<string> tags = new List<string>(); // Umožňuje upraviť načítanie podľa zadaných príznakov tags v classoch ktoré postupne dedia tento class.
                dataRecord.SetRecordValues(dbReader, tags);
                if (this.SynchronizationContext == null)
                    _list.Add(dataRecord);
                else
                    this.SynchronizationContext.Send(o => _list.Add(dataRecord), null);
                count++;

                // Ukonči načítanie údajov, ak bol načítaný maximálny limit.
                if (recordsLimit > 0 && count >= recordsLimit) break;
            }

            this.DBClient.Close();
            this.IsDataLoaded = true;
        }
        /// <summary>
        /// Vymazanie všetkých záznam v databázovej tabuľke.
        /// </summary>
        public void DeleteAll() 
        {
            this.DBClient.Open();

            // Vytvor dočasne pomocný objekt DataRecord, napr. pre zistenie názvu DB tabuľky.     
            DataRecord dataRecord = this.GetInitializedDataRecord();

            // Vymaž všetky záznamy v databáze a v objekte List.
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM {0}", dataRecord.TableName);
            if (!string.IsNullOrEmpty(this.FilterCondition))
                sql.AppendFormat(" WHERE {0}", this.FilterCondition);

            this.DBClient.ExecuteNonQuery(sql.ToString());

            if (this.SynchronizationContext == null)
                _list.Clear();
            else
                this.SynchronizationContext.Send(o => _list.Clear(), null);

            this.DBClient.Close();
        }
        /// <summary>
        /// Nájde index záznamu (objektu DataRecord) v objekte List, podľa zadaného identifikátora DataRecord.
        /// </summary>
        /// <param name="recordID">Jedinečný identifikátor objektu DataRecord.</param>
        public int FindListIndex(long recordID) 
        {
            int index = -1;
            if (recordID < 0) return index;
            foreach (DataRecord dataRecord in this.List) 
            {
                index++;
                if (recordID == dataRecord.ID) return index; 
            }
            return index;
        }
        /// <summary>
        /// Nájde objekt DataRecord podľa zadaného identifikátora.
        /// </summary>
        /// <param name="recordID">Jedinečný identifikátor objektu DataRecord.</param>
        public DataRecord FindDataRecord(long recordID) 
        {
            foreach (DataRecord dataRecord in this.List)
            {
                if (recordID == dataRecord.ID) 
                    return dataRecord;
            }
            return null;
        }


    }


}
