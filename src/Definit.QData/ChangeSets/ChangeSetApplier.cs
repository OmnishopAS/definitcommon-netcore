using Definit.QData.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Definit.QData.ChangeSets
{
    public class ChangeSetApplier<TEntity> where TEntity : class, new()
    {
        readonly IChangeSetContext _repoContext;
        readonly IQDataEntityModel _entityModel;

        readonly Dictionary<ChangeSetEntry, object> _appliedChangeSetEntries = new Dictionary<ChangeSetEntry, object>();

        public Dictionary<ChangeSetEntry, object> AppliedChangeSetEntries { get { return _appliedChangeSetEntries; } }

        public ChangeSetApplier(IChangeSetContext repoContext, IQDataEntityModel entityModel)
        {
            _repoContext = repoContext;
            _entityModel = entityModel;
        }

        public TEntity ApplyChangeSet(ChangeSetEntry changeSet)
        {
            ConvertKeys(typeof(TEntity), changeSet);

            TEntity entity;

            switch (changeSet.Operation)
            {
                case ChangeSetOperation.Insert:
                    entity = new TEntity();
                    ApplyChangeSetEntryInsert(changeSet, entity);
                    _repoContext.AddNewEntity(entity);
                    ProcessCompositeChildren(changeSet, entity);
                    break;
                case ChangeSetOperation.Update:
                    entity = (TEntity)FindEntity(typeof(TEntity), changeSet);
                    ApplyChangeSetEntryUpdate(changeSet, entity);
                    ProcessCompositeChildren(changeSet, entity);
                    break;
                case ChangeSetOperation.Delete:
                    entity = (TEntity)FindEntity(typeof(TEntity), changeSet);
                    ApplyChangeSetEntryDelete(changeSet, entity);
                    ProcessCompositeChildren(changeSet, entity);
                    _repoContext.DeleteEntity(entity);
                    break;
                case ChangeSetOperation.None:
                    entity = (TEntity)FindEntity(typeof(TEntity), changeSet);
                    ApplyChangeSetEntryNone(changeSet, entity);
                    ProcessCompositeChildren(changeSet, entity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Operation: " + changeSet.Operation + " is not a valid change set operation.");
            }

            return entity;
        }

        /// <summary>
        /// Adds ChangeSetEntry to _appliedChangeSetEntries.
        /// Sets properties on new entity based on newvalues in ChangeSetEntry.
        /// Performs concurrency check of oldvalues exists in ChangeSetEntry.
        /// </summary>
        /// <param name="entry">The changeset entry that will be applied</param>
        /// <param name="entity">The entity whose properties will be updated</param>
        private void ApplyChangeSetEntryInsert(ChangeSetEntry entry, object entity)
        {
            if (entry.Operation != ChangeSetOperation.Insert)
            {
                throw new ArgumentException("Operation of entry must be Insert but is: " + entry.Operation);
            }

            _appliedChangeSetEntries.Add(entry, entity);
            PropertyInfo[] properties = entity.GetType().GetProperties();
            var entityKeys = _entityModel.GetKeysForEntity(entity.GetType());
            ValidateKeys(entityKeys, entry.Keys);

            if (entry.OldValues != null && entry.OldValues.Any())
            {
                throw new ChangeSetInvalidException("OldValues must be empty for operation Insert");
            }

            int i = 0;
            foreach (var entityKey in entityKeys)
            {
                //Key value(s) may be null for insert of entities with server/database generated key value(s) 
                if (entry.Keys[i].Value != null)
                {
                    entityKey.PropertyInfo.SetValue(entity, entry.Keys[i].Value);
                }
                i++;
            }

            if (entry.NewValues == null)
            {
                throw new ChangeSetInvalidException("NewValues can not be null for operations Update or Insert");
            }

            UpdateValues(properties, entityKeys, entry.NewValues, entity);
        }

        /// <summary>
        /// Adds ChangeSetEntry to _appliedChangeSetEntries.
        /// Updates properties on entity based on newvalues in ChangeSetEntry.
        /// Performs concurrency check of oldvalues exists in ChangeSetEntry.
        /// </summary>
        /// <param name="entry">The changeset entry that will be applied</param>
        /// <param name="entity">The entity whose properties will be updated</param>
        private void ApplyChangeSetEntryUpdate(ChangeSetEntry entry, object entity)
        {
            if (entry.Operation != ChangeSetOperation.Update)
            {
                throw new ArgumentException("Operation of entry must be Update but is: " + entry.Operation);
            }

            _appliedChangeSetEntries.Add(entry, entity);
            PropertyInfo[] properties = entity.GetType().GetProperties();
            var entityKeys = _entityModel.GetKeysForEntity(entity.GetType());
            ValidateKeys(entityKeys, entry.Keys);

            if (entry.OldValues != null)
            {
                ValuesMatchDbCheck(properties, entry.OldValues, entity);
            }

            if (entry.NewValues == null)
            {
                throw new ChangeSetInvalidException("NewValues can not be null for operations Update or Insert");
            }

            UpdateValues(properties, entityKeys, entry.NewValues, entity);
        }

        /// <summary>
        /// Adds ChangeSetEntry to _appliedChangeSetEntries.
        /// Checks if ChangeSetEntry is a valid entry for delete.
        /// </summary>
        /// <param name="entry">The changeset entry that will be applied</param>
        /// <param name="entity">The entity whose properties will be updated</param>
        private void ApplyChangeSetEntryDelete(ChangeSetEntry entry, object entity)
        {
            if (entry.Operation != ChangeSetOperation.Delete)
            {
                throw new ArgumentException("Operation of entry must be Delete but is: " + entry.Operation);
            }

            _appliedChangeSetEntries.Add(entry, entity);
            PropertyInfo[] properties = entity.GetType().GetProperties();
            var entityKeys = _entityModel.GetKeysForEntity(entity.GetType());
            ValidateKeys(entityKeys, entry.Keys);

            if (entry.OldValues != null)
            {
                ValuesMatchDbCheck(properties, entry.OldValues, entity);
            }

            if (entry.NewValues != null && entry.NewValues.Any())
            {
                throw new ChangeSetInvalidException("NewValues must be empty for operation Delete");
            }
        }

        /// <summary>
        /// Adds ChangeSetEntry to _appliedChangeSetEntries.
        /// Validates keys in ChangeSetEntry.
        /// </summary>
        /// <param name="entry">The changeset entry that will be applied</param>
        /// <param name="entity">The entity whose properties will be updated</param>
        private void ApplyChangeSetEntryNone(ChangeSetEntry entry, object entity)
        {
            if (entry.Operation != ChangeSetOperation.None)
            {
                throw new ArgumentException("Operation of entry must be None but is: " + entry.Operation);
            }

            _appliedChangeSetEntries.Add(entry, entity);
            var entityKeys = _entityModel.GetKeysForEntity(entity.GetType());
            ValidateKeys(entityKeys, entry.Keys);
        }

        private void ProcessCompositeChildren(ChangeSetEntry changeSet, object entity)
        {
            if (changeSet.CompositeCollections == null) { return; }

            foreach (var compositeCollection in changeSet.CompositeCollections)
            {
                var entryList = compositeCollection.Value;

                //TODO: Check that this is an actual allowed composition (from composite attribute or similar)
                PropertyInfo navEntityPropInfo = entity.GetType().GetRuntimeProperty(compositeCollection.Key);
                IList navEntities = (IList)navEntityPropInfo.GetValue(entity);
                Type entityType = navEntityPropInfo.PropertyType.GenericTypeArguments[0];

                if (navEntities == null)
                {
                    Type genericType = typeof(List<>).MakeGenericType(entityType);
                    navEntities = (IList)Activator.CreateInstance(genericType);
                    navEntityPropInfo.SetValue(entity, navEntities);
                }

                foreach (ChangeSetEntry entry in entryList)
                {
                    ConvertKeys(entityType, entry);

                    if (entry.Operation == ChangeSetOperation.Insert)
                    {
                        var newNavEntity = Activator.CreateInstance(entityType);
                        ApplyChangeSetEntryInsert(entry, newNavEntity);
                        ProcessCompositeChildren(entry, newNavEntity);
                        navEntities.Add(newNavEntity);
                    }
                    else if (entry.Operation == ChangeSetOperation.Update)
                    {
                        object navEntity = FindEntity(entityType, entry);
                        ApplyChangeSetEntryUpdate(entry, navEntity);
                        ProcessCompositeChildren(entry, navEntity);
                    }
                    else if (entry.Operation == ChangeSetOperation.Delete)
                    {
                        object navEntity = FindEntity(entityType, entry);
                        ApplyChangeSetEntryDelete(entry, navEntity);
                        ProcessCompositeChildren(entry, navEntity);
                        _repoContext.DeleteEntity(navEntity);
                        navEntities.Remove(navEntity);
                    }
                    else if (entry.Operation == ChangeSetOperation.None)
                    {
                        object navEntity = FindEntity(entityType, entry);
                        ApplyChangeSetEntryNone(entry, navEntity);
                        ProcessCompositeChildren(entry, navEntity);
                    }
                }

            }
        }

        //Converts all entries in keys array to correct type, based on EntityType's keys.
        //Could/should be done during deserialization instead?
        private void ConvertKeys(Type entityType, ChangeSetEntry entry)
        {
            var entityKeys = _entityModel.GetKeysForEntity(entityType);
            var sortedKeys = new List<KeyValuePair<string, object>>(entityKeys.Count());
            foreach (var entityKey in entityKeys)
            {
                var entryKey = entry.Keys.FirstOrDefault(k => k.Key == entityKey.Name);
                var keyValue = entryKey.Value;
                if (entryKey.Value != null)
                {
                    keyValue = ChangeSetHelper.ConvertToType(entityKey.PropertyInfo.PropertyType, entryKey.Value);
                }
                sortedKeys.Add(new KeyValuePair<string, object>(entityKey.Name, keyValue));
            }
            entry.Keys = sortedKeys;
        }

        private object FindEntity(Type entityType, ChangeSetEntry entry)
        {
            var entity = _repoContext.LoadEntity(entityType, entry.Keys.Select(kvp => kvp.Value).ToArray());

            if (entity == null)
                throw new ChangeSetException("ChangeSetEntry not found in " + entityType.Name + ".");

            return entity;
        }

        private void ValidateKeys(EntityKey[] entityKeys, IList<KeyValuePair<string, object>> entryKeys)
        {
            //TODO: Also check that type, name and index of key matches
            if (entityKeys.Length != entryKeys.Count())
                throw new ChangeSetInvalidException("Invalid Keys array. Expected " + entityKeys.Length + " entries, found " + entryKeys.Count() + " entries.");
        }

        private void UpdateValues(PropertyInfo[] properties, EntityKey[] entityKeys, IList<KeyValuePair<string, object>> values, object entity)
        {
            foreach (KeyValuePair<string, object> kvPair in values)
            {
                var propertyInfo = properties.FirstOrDefault(p => p.Name == kvPair.Key);

                if (propertyInfo == null)
                {
                    //TODO: Midlertidig løsning fordi Helicon tar med $_visited
                    continue;
                    //throw new ChangeSetInvalidException("Property '" + kvPair.Key + "' does not exist on entity " + entity.GetType().FullName);
                }

                if (entityKeys.Any(x => x.Name == propertyInfo.Name))
                {
                    //TODO: Midlertidig løsning fordi Helicon tar med keys i NewValues
                    continue;
                    //throw new ChangeSetInvalidException("Property '" + kvPair.Key + "' is a key property and can not be part of NewValues for entity " + entity.GetType().FullName);
                }

                if (!propertyInfo.CanWrite)
                {
                    throw new ChangeSetInvalidException("Property '" + kvPair.Key + "' on entity " + entity.GetType().FullName + " is not writeable");
                }

                //TODO: Check that property is writeable for client 
                //      - Readonly: DateCreated / DateLastChange
                //      -         : Computed properties (RRPIncVAT)
                var value = kvPair.Value;
                if (value != null && propertyInfo.PropertyType != value.GetType())
                    value = ChangeSetHelper.ConvertToType(propertyInfo.PropertyType, value);
                propertyInfo.SetValue(entity, value);
            }
        }

        private bool AreValuesEqual(object propertyValue, object changeSetValue, Type propertyType)
        {
            if (propertyValue == null)
            {
                return changeSetValue == null;
            }

            if (propertyType.IsEnum)
            {
                return propertyValue.Equals(Enum.Parse(propertyType, changeSetValue.ToString()));
            }

            return ChangeSetHelper.ConvertToType(propertyType, changeSetValue).Equals(propertyValue);
        }

        private void ValuesMatchDbCheck(PropertyInfo[] properties, IList<KeyValuePair<string, object>> values, object entity)
        {
            foreach (KeyValuePair<string, object> kvpValue in values)
            {
                PropertyInfo propInfo = properties.Where(p => p.Name == kvpValue.Key).FirstOrDefault();
                if (propInfo == null)
                    throw new ChangeSetInvalidException("Key " + kvpValue.Key + " in ChangeSetEntry.OldValues not found as property on entity.");

                if (!AreValuesEqual(propInfo.GetValue(entity), kvpValue.Value, propInfo.PropertyType))
                {
                    throw new ChangeSetConcurrencyException("Old changeset value for " + kvpValue.Key + " does not match saved db value.");
                }
            }
        }
    }
}

