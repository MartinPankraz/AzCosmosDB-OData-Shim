namespace GenericODataWebAPI.Cosmos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using GenericODataWebAPI.Core;

    
    public class CosmosDBRepository<T> : IDataRepository<T> where T : class , IDocumentWithId
    {
       
        private string Endpoint = "";
        private string Key = "";
        private string DatabaseId = "";
        private string CollectionId = "";
        private CosmosClient client;
        private Container container; 
        private Database database; 
        private bool DBCheckDone = false;

        public CosmosDBRepository(string Endpoint, string Key, string DatabaseId, string CollectionId)
        { 
            this.Endpoint = Endpoint;
            this.Key = Key;
            this.DatabaseId = DatabaseId;
            this.CollectionId = CollectionId;
            this.client = new CosmosClient(this.Endpoint, this.Key);
            DBCheckDone = CheckDBAndCollectionExist().Result;
        }

        public async Task<bool> CheckDBAndCollectionExist()
        {
            database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
            container = await database.CreateContainerIfNotExistsAsync(CollectionId, "/id", 400);
            return true;
        }

        public async Task<T> GetItemAsync(string id)
        {
            return await container.ReadItemAsync<T>(partitionKey: new PartitionKey(id), id: id);
        }

        public async Task<IEnumerable<T>> GetItemsAsync()
        {

            List<T> Ts = new List<T>();
            using (FeedIterator<T> resultSet = container.GetItemQueryIterator<T>(queryDefinition: null))
            {
                while (resultSet.HasMoreResults)
                {
                    // Ts.Add(await resultSet.ReadNextAsync()).First();

                    FeedResponse<T> response = await resultSet.ReadNextAsync();
                    T sale = response.First();
                    Ts.AddRange(response);
                }
            }
            return Ts;
        }
        
        public async Task<string> CreateItemAsync(T item)
        {
            ItemResponse<T> response = await container.CreateItemAsync(item, new PartitionKey(item.id));
            return response.ETag;
        }

        public async Task<string> UpdateItemAsync(string id, T item)
        {
            ItemResponse<T> response = await container.UpsertItemAsync(item, new PartitionKey(item.id));
            return response.ETag;
        }

        public async Task DeleteItemAsync(string id)
        {
            ItemResponse<T> response = await container.DeleteItemAsync<T>(partitionKey: new PartitionKey(id), id: id);
        }
    }
}
