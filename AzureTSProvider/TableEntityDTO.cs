using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AzureTSProvider
{
    public class TableEntityDTO   : DynamicObject,ITableEntity
    {
        private bool isDirty = false;
        #region ITableEntity properties

        // Summary:
        //     Gets or sets the entity's current ETag. Set this value to '*' in order to
        //     blindly overwrite an entity as part of an update operation.
        public string ETag { get; set; }
        //
        // Summary:
        //     Gets or sets the entity's partition key.
        public string PartitionKey { get; set; }
        //
        // Summary:
        //     Gets or sets the entity's row key.
        public string RowKey { get; set; }
        //
        // Summary:
        //     Gets or sets the entity's time stamp.
        public DateTimeOffset Timestamp { get; set; }
        #endregion

        // Use this Dictionary store table's properties. 
        public IDictionary<string, EntityProperty> properties { get; private set; }

        public TableEntityDTO()
        {
            properties=new Dictionary<string,EntityProperty>();
        }

        public TableEntityDTO(string PartitionKey, string RowKey)
        {
            this.PartitionKey = PartitionKey;
            this.RowKey = RowKey;
            properties = new Dictionary<string, EntityProperty>();
        }

        public bool IsDirty{ 
            get
            {
                return isDirty;   
            }

            set
            {
                isDirty = value;
            }
        }

        #region override DynamicObject's mehtods
        public override bool TryGetMember(GetMemberBinder binder, out object result)
	    {
            if (!properties.ContainsKey(binder.Name))
                properties.Add(binder.Name, ConvertToEntityProperty(binder.Name, null));
            result = properties[binder.Name];
		    return true;
	    }
 
	    public override bool TrySetMember(SetMemberBinder binder, object value)
	    {
            EntityProperty property = ConvertToEntityProperty(binder.Name, value);

            if (properties.ContainsKey(binder.Name))
                properties[binder.Name] = property;
            else
                properties.Add(binder.Name, property);
            
		    return true;
	    }

        public bool TrySetMember(string binder, object value)
        {
            EntityProperty property = ConvertToEntityProperty(binder, value);

            if (properties.ContainsKey(binder))
                properties[binder] = property;
            else
                properties.Add(binder, property);

            return true;
        }
	
	    #endregion
	
	    #region ITableEntity implementation
	
	    public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
	    {
		    this.properties = properties;
	    }
 
	    public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
	    {
		    return this.properties;
	    }
	
	    #endregion
	
	    /// <summary>
        /// Convert object value to EntityProperty.
	    /// </summary>
	    private EntityProperty ConvertToEntityProperty(string key, object value)
	    {
            if (value == null) return new EntityProperty((string)null);
		    if (value.GetType() == typeof(byte[])) 
                return new EntityProperty((byte[])value);
		    if (value.GetType() == typeof(bool)) 
                return new EntityProperty((bool)value);
		    if (value.GetType() == typeof(DateTimeOffset)) 
                return new EntityProperty((DateTimeOffset)value);
		    if (value.GetType() == typeof(DateTime)) 
                return new EntityProperty((DateTime)value);
		    if (value.GetType() == typeof(double)) 
                return new EntityProperty((double)value);
		    if (value.GetType() == typeof(Guid)) 
                return new EntityProperty((Guid)value);
		    if (value.GetType() == typeof(int)) 
                return new EntityProperty((int)value);
		    if (value.GetType() == typeof(long)) 
                return new EntityProperty((long)value);
		    if (value.GetType() == typeof(string)) 
                return new EntityProperty((string)value);
		    throw new Exception("This value type" + value.GetType() + " for " + key);
            throw new Exception(string.Format("This value type {0} is not supported for {1}",key));
	    }
       
         /// <summary>
         /// Get the edm type, if the type is not a edm type throw a exception.
         /// </summary>
	    private Type GetType(EdmType edmType)
	    {
		    switch (edmType)
		    {
			    case EdmType.Binary : 
                    return typeof(byte[]);
			    case EdmType.Boolean : 
                    return typeof(bool);
			    case EdmType.DateTime : 
                    return typeof(DateTime);
			    case EdmType.Double : 
                    return typeof(double);
			    case EdmType.Guid : 
                    return typeof(Guid);
			    case EdmType.Int32 : 
                    return typeof(int);
			    case EdmType.Int64 : 
                    return typeof(long);
			    case EdmType.String : 
                    return typeof(string);
			    default: throw new TypeLoadException(string.Format("not supported edmType:{0}" ,edmType));
		    }
	    }
        }
}

