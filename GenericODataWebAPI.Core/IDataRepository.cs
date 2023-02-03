using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Deltas;

namespace GenericODataWebAPI.Core
{
    public interface IDataRepository<T> where T : class, IDocumentWithId
    {
        
        Task<T> CreateItemAsync(T item);
 
        Task DeleteItemAsync(string id);

        Task<T> GetItemAsync(string id);

        Task<IEnumerable<T>> GetItemsAsync();

        Task<T> UpdateItemAsync(string id, T item);

        Task<T> PatchItemAsync(string id, Delta<T> item);

    }
}
