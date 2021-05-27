using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GenericODataWebAPI.Core
{
    public interface IDataRepository<T> where T : class, IDocumentWithId
    {
        
        Task<string> CreateItemAsync(T item);
 
        Task DeleteItemAsync(string id);

        Task<T> GetItemAsync(string id);

        Task<IEnumerable<T>> GetItemsAsync();

        Task<string> UpdateItemAsync(string id, T item);

        Task<string> PatchItemAsync(string id, T item);

    }
}
