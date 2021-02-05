using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.Scripts
{
    public interface IResourceCacheService// : IDisposable
    {
        T Load<T>(string path) where T : Object;
        IList<T> LoadAll<T>(IList<string> paths) where T : Object;
        
        void Unload(string path);
        
        void UnloadAll();

        int CacheCount { get; }
        
        IList<string> CachePaths { get; }
    }

    public class ResourceCacheService : IResourceCacheService
    {
        readonly Dictionary<string, Object> _cache = new Dictionary<string, Object> (512);

        public int CacheCount => _cache.Count;

        public IList<string> CachePaths => _cache.Keys.ToList();

        /// <summary>
        /// Return loaded resource from cache or load it. Important: if you request resource with one type,
        /// you cant get it for same path and different type.
        /// </summary>
        /// <param name="path">Path to loadable resource relative to "Resources" folder.</param>
        public T Load<T>(string path) where T : Object 
        {
            Object asset;
            if (!_cache.TryGetValue (path, out asset)) {
                asset = Resources.Load<T>(path);
                if (asset != null) {
                    _cache[path] = asset;
                }
            }
            return asset as T;
        }
        
        public IList<T> LoadAll<T>(IList<string> paths) where T : Object 
        {
            var result = new List<T>(paths.Count);
            foreach (var path in paths)
            {
                result.Add(Load<T>(path));
            }
            return result;
        }
        
        public async Task<T> LoadAsync<T>(string path) where T : Object 
        {
            Object asset;
            if (!_cache.TryGetValue (path, out asset)) {
                asset = await Resources.LoadAsync<T>(path);
                if (asset != null) {
                    _cache[path] = asset;
                }
            }
            return asset as T;
        }
        
        public async Task<IList<T>> LoadAllAsync<T>(IList<string> paths) where T : Object 
        {
            var result = new List<T>(paths.Count);
            foreach (var path in paths)
            {
                result.Add(await LoadAsync<T>(path));
            }
            return result;
        }

        /// <summary>
        /// Force unload resource. Use carefully.
        /// </summary>
        /// <param name="path">Path to loadable resource relative to "Resources" folder.</param>
        public void Unload(string path) 
        {
            Object asset;
            if (_cache.TryGetValue (path, out asset)) 
            {
                _cache.Remove(path);
                Resources.UnloadAsset(asset);
            }
        }

        public void UnloadAll()
        {
            foreach (var pair in _cache)
            {
                Resources.UnloadAsset(pair.Value);
            }
            _cache.Clear();
        }

        #region IDisposable

        private bool _disposed = false;
        
        //~ResourceCacheService() => Dispose(false);

        //public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                UnloadAll();
            }

            _disposed = true;
        }

        #endregion
    }
}