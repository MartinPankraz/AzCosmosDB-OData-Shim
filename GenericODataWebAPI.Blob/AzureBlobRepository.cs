namespace GenericODataWebAPI.Blob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using GenericODataWebAPI.Core; 
    using System.Text.Json;

    public class AzureBlobRepository<T> : IDataRepository<T> where T : class, IDocumentWithId
    {
        private BlobContainerClient bc1;
        public AzureBlobRepository()
        {
            // BlobContainerClient bc
            // bc1 = bc;
        }

        public async Task<string> CreateItemAsync(T item)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetItemAsync(string id)
        {
            return default(T); // return await JsonSerializer.DeserializeAsync<T>(await bc1.GetBlobClient($"{id}.blob").Download().Content);

        }

        public async Task<IEnumerable<T>> GetItemsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> UpdateItemAsync(string id, T item)
        {
            throw new NotImplementedException();
        }

        public async Task<string> PatchItemAsync(string id, T item)
        {
            throw new NotImplementedException();
        }
    }
}