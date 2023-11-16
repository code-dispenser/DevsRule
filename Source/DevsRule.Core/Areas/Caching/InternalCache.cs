using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using System.Collections.Concurrent;

namespace DevsRule.Core.Areas.Caching;

internal class InternalCache
{

    private readonly ConcurrentDictionary<CacheKey, SemaphoreSlim>    _lockManager = new();
    private readonly ConcurrentDictionary<CacheKey, CacheItem>        _cachedItems = new();

    public bool ContainsItem(string itemKey, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)

        => _cachedItems.ContainsKey (new CacheKey(itemKey, tenantID, cultureID));
    

    public bool TryGetItem<T>(string itemKey, out T? cacheItem, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKey = new CacheKey(itemKey, tenantID, cultureID);
        
        if(true == _cachedItems.TryGetValue(cacheKey, out var itemInCache))
        {
            cacheItem = (T)itemInCache.Value;
            return true;
        }
        
        cacheItem = default;
        return false;
    }

    public T GetOrAddItem<T>(string itemKey, Type typeParam, Func<Type, T> createItemForCache, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKey        = new CacheKey(itemKey, tenantID, cultureID);
        var semiphoreSlim   = _lockManager.GetOrAdd(cacheKey, new SemaphoreSlim(1, 1));
        var lockAquired     = false;//incase of exception trying to get a lock we dont want to do a release.

        try
        {   
            semiphoreSlim.Wait(100);
            
            lockAquired = true;

            Func<CacheKey, CacheItem> buildCacheItem = (key) => new CacheItem(createItemForCache(typeParam)!);

            CacheItem cachedItem =  _cachedItems.GetOrAdd(cacheKey, buildCacheItem);

            return (T)cachedItem.Value;
        }
        finally
        {
            if (true == lockAquired) semiphoreSlim.Release();
        }


    }
    public T GetOrAddItem<T>(string itemKey, Func<T> createItemForCache, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKey        = new CacheKey(itemKey, tenantID, cultureID);
        var semiphoreSlim   = _lockManager.GetOrAdd(cacheKey, new SemaphoreSlim(1, 1));
        var lockAquired     = false;

        semiphoreSlim.Wait();
        lockAquired = true;

        try
        {
            Func<CacheKey, CacheItem> buildCacheItem = (key) => new CacheItem(createItemForCache()!);

            CacheItem cachedItem = _cachedItems.GetOrAdd(cacheKey, buildCacheItem);

            return (T)cachedItem.Value;

        }
        finally
        {
            if (true == lockAquired) semiphoreSlim.Release();
        }

    }

    public void AddOrUpdateItem<T>(string itemKey, T itemToCache, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID) where T: notnull
    {
        var cacheKey        = new CacheKey(itemKey, tenantID, cultureID);
        var semiphoreSlim   = _lockManager.GetOrAdd(cacheKey, new SemaphoreSlim(1, 1));
        var lockAquired     = false;

        semiphoreSlim.Wait();
        lockAquired = true;

        try
        {
            Func<CacheKey, CacheItem, T, CacheItem> updateItem = (cacheKey, existingItem, T) =>
            {
                try
                {
                    if (existingItem.Value is IDisposable) ((IDisposable)existingItem.Value).Dispose();
                }
                catch { }//decided just to squash this error as its not likely to occur. It would have to be a custom evaluator being updated that throws an error in its dispose method assuming it has one;

                //catch (Exception ex) { throw new DisposingRemovedItemException(String.Format(GlobalStrings.Disposing_Removed_Item_Exception_Message, cacheKey.ItemName), ex); }

                return new CacheItem(T);

            };

            _ = _cachedItems.AddOrUpdate<T>(cacheKey, (cacheKey, T) => new CacheItem(T), updateItem, itemToCache);
        }
        finally
        {
            if (true == lockAquired) semiphoreSlim.Release();
        }
    }

    public void RemoveItem(string itemKey, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKey = new CacheKey(itemKey, tenantID, cultureID);

        if (true == _cachedItems.TryRemove(cacheKey, out var cachedItem))
        {
            try
            {
                if (cachedItem.Value is IDisposable) ((IDisposable)cachedItem.Value).Dispose();
            }
            catch(Exception ex) { throw new DisposingRemovedItemException(String.Format(GlobalStrings.Disposing_Removed_Item_Exception_Message, cacheKey.ItemName),ex); }
        }
    }
}

