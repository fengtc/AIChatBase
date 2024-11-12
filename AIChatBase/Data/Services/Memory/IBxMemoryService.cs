using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatBase.Data.Services.Memory
{
    public interface IBxMemoryService
    {
        Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> func, TimeSpan? expr = null);
        T GetOrCreate<T>(object key, Func<T> func, TimeSpan? expr = null);
    }
}
