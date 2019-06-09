using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Definit.QData.ChangeSets
{
    public class ChangeSetEntry
    {
        public ChangeSetOperation Operation { get; set; }
        public IList<KeyValuePair<string, object>> Keys { get; set; }
        public IList<KeyValuePair<string, object>> OldValues { get; set; }
        public IList<KeyValuePair<string, object>> NewValues { get; set; }

        /// <summary>
        /// Key should point to property on entity. This property must implement IList<T>.
        /// </summary>
        public IList<KeyValuePair<string, IList<ChangeSetEntry>>> CompositeCollections { get; set; }
    }

    public enum ChangeSetOperation
    {
        None,
        Insert,
        Update,
        Delete
    }

}
