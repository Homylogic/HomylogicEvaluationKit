/* DATA RECORD
 * 
 * Používa sa pre dedenie čím vytvorí objekt s databázovým pripojením.
 * Obsahuje vlastností previazané na databázové stĺpce.
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using X.Basic;

namespace X.Data.Factory
{
    public abstract class DataRecord
    {
        public enum RecordStateTypes { New, Edit }
        /// <summary>
        /// Vráti stav záznamu, teda príznak, ktorý určuje aká databázová operácia sa bude vykonávať.
        /// </summary>
        public RecordStateTypes RecordState { get; private set; } = RecordStateTypes.New;
        /// <summary>
        /// Datalist v ktorom sa nachádza tento objekt DataRecord.
        /// </summary>
        public DataList ParentDataList { get; set; }
        /// <summary>
        /// Získani pripojenia k DB z horného classu (ktorý dedí tento class).
        /// </summary>
        public abstract DBClient DBClient { get; }
        /// <summary>
        /// Názov databázovej tabuľky.
        /// </summary>
        public abstract string TableName { get; }

        protected Int64 _id;
        /// <summary>
        /// Obsahuje databázový identifikátor záznamu.
        /// </summary>
        public Int64 ID { get { return _id; } }
        /// <summary>
        /// Nastavenie vlastností objektu DataRecord podľa načítaného databázového DataReadera.
        /// </summary>
        public virtual void SetRecordValues(DBReader dbReader, List<string> tags) 
        {
            _id =  dbReader.GetInt64("id");

            // Zmeň stav na editáciu existujúceho záznamu, pretože údaje boli načítané a nastavené z databázy.
            RecordState = RecordStateTypes.Edit;
        }
        /// <summary>
        /// Vráti dotaz pre vloženie nového zázanmu podľa vlastností z najvyššieho objektu podľa dedenia.
        /// </summary>
        public abstract string SqlInsert(Management.SqlConvert q, List<string> tags);
        /// <summary>
        /// Vráti dotaz pre uloženie zázanmu podľa vlastností najvyššieho objektu podľa dedenia.
        /// </summary>
        public abstract string SqlUpdate(Management.SqlConvert q, List<string> tags);
        /// <summary>
        /// Vykoná kotrolu zadaných hodnôt vlastností, používa sa (manuálne volanie) pred uložením do databázy.
        /// </summary>
        public virtual XException Validate() { return new XException(); }

        protected enum AddToListTypes { AddLast, InsertFirst };
        /// <summary>
        /// Spôsob pridania objektu DataRecord do zoznamu DataList (BindingList, ktorý sa nachádza v pamäti).
        /// </summary>
        protected AddToListTypes AddToListType = AddToListTypes.AddLast;

        /// <summary>
        /// Uloží aktuaálne údaje objektu do databázy. Nový záznam pridá do parent DataListu.
        /// </summary>
        public virtual void Save() 
        {
            this.DBClient.Open();

            // Aktualizovanie databázy. 
            Management.SqlConvert q = new Management.SqlConvert(this.DBClient.ClientType);
            List<string> tags = new List<string>(); // Umožňuje upraviť SQL podľa zadaných príznakov tags v classoch ktoré postupne dedia tento class.
            string sql;
            switch (this.RecordState) 
            {
                case RecordStateTypes.New: 
                    sql = this.SqlInsert(q, tags); 
                    break;
                case RecordStateTypes.Edit:
                    if (_id < 0) throw new InvalidOperationException("DataRecord identifier is zero value.");
                    sql = this.SqlUpdate(q, tags);
                    sql += string.Format(" WHERE id = {0}", _id);
                    break;
                default:
                    throw new InvalidOperationException("Invalid DataRecord state.");
            }
            this.DBClient.ExecuteNonQuery(sql);

            // Nastavenie identifikátora podľa datazbázy.
            if (this.RecordState == RecordStateTypes.New)
                _id = this.DBClient.GetLastInsertedRowID();

            // Update parent DataLIst collection after saving new DataRecord.
            // Remove RecordsLimit overflow items.
            if (this.ParentDataList != null && this.RecordState == RecordStateTypes.New) 
            {
                if (AddToListType == AddToListTypes.AddLast) {
                    this.ParentDataList.List.Add(this);
                    if (this.ParentDataList.List.Count > this.ParentDataList.RecordsLimit)
                        this.ParentDataList.List.RemoveAt(0);
                }
                else {
                    this.ParentDataList.List.Insert(0, this);
                    if (this.ParentDataList.List.Count > this.ParentDataList.RecordsLimit)
                        this.ParentDataList.List.RemoveAt(this.ParentDataList.List.Count - 1);
                }
            }

            // Zmeň stav na editáciu existujúceho záznamu, pretože bol uložený (napr. ak bol záznam v režime nový).
            RecordState = RecordStateTypes.Edit;
        }
        /// <summary>
        /// Načíta údaje z databázy a nastaví vlastnosti tohoto objektu, podľa zadaného identifikátora.
        /// </summary>
        /// <param name="id">Jedinečný identifikátor záznamu.</param>
        public virtual void Load(Int64 id) 
        {
            this.DBClient.Open();

            string sql = $"SELECT * FROM {this.TableName} WHERE id = {id}";
            using DBReader dbReader = this.DBClient.ExecuteReader(sql);

            while (dbReader.Read())
            {
                List<string> tags = new List<string>(); // Umožňuje upraviť načítanie podľa zadaných príznakov tags v classoch ktoré postupne dedia tento class.
                this.SetRecordValues(dbReader, tags);
                break; // Nemalo by nikdy nastať, pretože sa načítava vždy len jeden záznam podľa ID.
            }
        }
        /// <summary>
        /// Aktualizuje už načítané údaje z databázy, podľa ID aktualne načítaného záznamu.
        /// </summary>
        public void Reload() 
        {
            if (_id == 0) return;
            this.Load(_id);
        }
        /// <summary>
        /// Vymaže záznam z databázy podľa zadaného ID (neaktualizuje parent DataList).
        /// </summary>
        /// <param name="id">Jedinečný identifikátor záznamu.</param>
        public virtual void Delete(Int64 id) 
        {
            if (id < 0) throw new ArgumentNullException("Identifier is zero value.");

            string sql = $"DELETE FROM {this.TableName} WHERE id = {id}";
            this.DBClient.Open();
            this.DBClient.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// Vymaže tento záznam z databázy a aktualizuje parent DataList.
        /// </summary>
        public void Delete() 
        {
            if (_id < 0) throw new InvalidOperationException("DataRecord identifier is zero value.");

            this.Delete(_id);
            if (this.ParentDataList != null)
                this.ParentDataList.List.Remove(this);
        }

    }
}
