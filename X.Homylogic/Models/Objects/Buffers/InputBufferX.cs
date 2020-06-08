/* HOMYLOGIC INPUT BUFFER X
 * 
 * Obsahuje údaje ktoré boli načítané napr. zo zariadenia.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;

namespace X.Homylogic.Models.Objects.Buffers
{
    public sealed class InputBufferX : BufferX
    {
        const string TITLE = "Input buffer";

        #region --- DATA PROPERTIES ---

        public Int32 Tag { get; set; } = 0;

        #endregion

        #region --- DATA RECORD ---

        public const string TABLE_NAME = "buffersInput";
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "tag";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}", (Int32)this.Tag);
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("tag = {0}", (Int32)this.Tag);
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override void Save()
        {
            // Nastaviť relačný názov zaridenia podľa zadaného identifikátor.
            this.Name = DeviceXList.GetRelationDeviceName(this.DeviceID);
            base.Save();
        }

        #endregion

        public InputBufferX() { base.BufferType = BufferTypes.Input; this.AddToListType = AddToListTypes.InsertFirst; }

    }


}
