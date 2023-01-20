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

        /// <summary>
        /// Mandatory for update/delete. Identifies the entity to be updated or deleted.
        /// Ignored for inserts (if entitytype to be inserted support client-supplied keys, client sets the key field(s) as part of NewValues)
        /// </summary>
        //Rename to Key?
        public IList<KeyValuePair<string, object>> Keys { get; set; }


        //Rename to Values?
        public IList<KeyValuePair<string, object>> NewValues { get; set; }

        //Ignored for insert. Can be used as concurrency check for delete/update operations.
        //For all fields that client includes in OldValues, server will compare the supplied value against the current value in database and abort the request if values are different.
        public IList<KeyValuePair<string, object>> OldValues { get; set; }
        

        /// <summary>
        /// Key is name of property on entity. This property must implement IList<T> and must be an QData CompositeCollection.
        /// Value is list of of ChangeSetEntry, where each ChangeSetEntry represents an entity belonging to this CompositeCollection that should be added, updated or removed.
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

    //public class ChangeSetEntries
    //{
    //    public string Type { get; set; }

    //}
}
