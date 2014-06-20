using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureTSProvider
{
    public class TableSet<TEntity> : ITableSet
    where TEntity : class,
        new()
    {
        private string partitionKey;
        private string tableName;
        private string connectionString;

        internal CloudTableClient tableClient;
        internal CloudTable table;

        internal List<TEntity> mappedList;
        internal List<TableEntityDTO> internalList;

        public TableSet(string connectionString, string tableName)
        {
            this.partitionKey = typeof(TEntity).Name;
            this.tableName = tableName;
            this.connectionString = connectionString;

            //pluralise the partition key (because basically it is the 'table' name).
            if (partitionKey.Substring(partitionKey.Length - 1, 1).ToLower() == "y")
                partitionKey = partitionKey.Substring(0, partitionKey.Length - 1) + "ies";

            if (partitionKey.Substring(partitionKey.Length - 1, 1).ToLower() != "s")
                partitionKey = partitionKey + "s";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();

            //initialise stuff
            mappedList = new List<TEntity>();
            internalList = new List<TableEntityDTO>();
        }

        public virtual TEntity GetByID(object id)
        {
            var query = new TableQuery().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id.ToString()));
            var dto = table.ExecuteQuery(query).First();
            TEntity mapped = StripDTO(dto);

            return mapped;
        }

        public virtual List<TEntity> GetAll()
        {
            var query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)); //get all customers - because Customer is our partition key
            var result = table.ExecuteQuery(query).ToList();

            foreach (var item in result)
            {
                mappedList.Add(StripDTO(item));
            }
            return mappedList;
        }

        public virtual void Insert(TEntity entity)
        {
            //mappedList.Add(entity);
            TableEntityDTO mapped = CreateDTO(entity);
            mapped.IsDirty = true;

            internalList.Add(mapped);
            
            //TableOperation insertOperation = TableOperation.Insert(mapped);
            //table.Execute(insertOperation);

        }

        public virtual int Commit()
        {
            var batch = new TableBatchOperation();

            //can only insert 100 rows at a time.
            int count = 0;
            int commitCount = 0;
            int maxBatch = 100;

            if (internalList.Count == 1)
            {
                //if we only have one item just perform a single (quicker) commit.
                TableOperation insertSingle = TableOperation.Insert(internalList.First());
                table.Execute(insertSingle);
                commitCount = 1;
            }
            else
            {
                foreach (var e in internalList.Where(i => i.IsDirty))
                {
                    //TableEntityDTO mapped = CreateDTO(e);
                    if (count < maxBatch)
                    {
                        batch.Insert(e);
                        count++;
                        commitCount++;
                    }
                    else
                    {
                        table.ExecuteBatch(batch);
                        batch = new TableBatchOperation();
                        count = 0;
                    }
                }

                if (batch.Count > 0)
                {
                    table.ExecuteBatch(batch);
                }
            }

            return commitCount;
        }


        #region object mapping
        dynamic CreateDTO(object a)
        {
            TableEntityDTO dto = new TableEntityDTO();
            object rowKey = null;

            Type t1 = a.GetType();
            Type t2 = dto.GetType();




            //now set all the entity properties
            foreach (System.Reflection.PropertyInfo p in t1.GetProperties())
            {
                 Type t = p.PropertyType;

                 bool isNested = t.IsNested;
                 bool isEnum = t.IsEnum;
                 bool isGeneric = t.IsGenericType;
                 bool isValueType = t.IsValueType;
                 bool isClass = t.IsClass;

                 if (t.IsGenericType && typeof(ICollection<>).IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                     t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)) || t.IsClass)
                 {
                     dto.TrySetMember(p.Name, JsonConvert.SerializeObject(p.GetValue(a, null)));
                 }
                 else
                 {
                     dto.TrySetMember(p.Name, p.GetValue(a, null) == null ? "" : p.GetValue(a, null));
                     if (IsId(p.Name))
                         rowKey = p.GetValue(a, null);

                 }
            }

            if (rowKey == null)
                rowKey = Guid.NewGuid();

            dto.RowKey = rowKey.ToString();
            dto.PartitionKey = partitionKey;
            dto.Timestamp = DateTime.Now;


            return dto;
        }

        TEntity StripDTO(Microsoft.WindowsAzure.Storage.Table.DynamicTableEntity a)
        {
            TEntity result = new TEntity();


            Type t1 = result.GetType();
            var dictionary = (IDictionary<string, EntityProperty>)a.Properties;

            foreach (PropertyInfo p1 in t1.GetProperties())//for each property in the entity,
            {
                foreach (var value in dictionary)//see if we have a correspinding property in the DTO
                {
                    if (p1.Name == value.Key)
                    {
                        Type t = p1.PropertyType;
                        if (t.IsPrimitive || t == typeof(string))
                        {
                            p1.SetValue(result, GetValue(value.Value));
                        }
                        else if (t.IsGenericType && typeof(ICollection<>).IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                            t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)) || t.IsClass)
                        {
                            var customClass = JsonConvert.DeserializeObject(value.Value.StringValue, t);
                            p1.SetValue(result, customClass);
                        }
                    }
                }

            }

            return result;
        }

        private object GetValue(EntityProperty source)
        {
            switch (source.PropertyType)
            {
                case EdmType.Binary:
                    return (object)source.BinaryValue;
                case EdmType.Boolean:
                    return (object)source.BooleanValue;
                case EdmType.DateTime:
                    return (object)source.DateTimeOffsetValue;
                case EdmType.Double:
                    return (object)source.DoubleValue;
                case EdmType.Guid:
                    return (object)source.GuidValue;
                case EdmType.Int32:
                    return (object)source.Int32Value;
                case EdmType.Int64:
                    return (object)source.Int64Value;
                case EdmType.String:
                    return (object)source.StringValue;
                default: throw new TypeLoadException(string.Format("not supported edmType:{0}", source.PropertyType));
            }
        }

        private bool IsId(string candidate)
        {
            bool result = false;

            if (candidate.ToLower() == "id")
                result = true;

            if (candidate.ToLower().Substring(candidate.Length - 2, 2) == "id")
                result = true;

            return result;
        }

        # endregion

    }

}
