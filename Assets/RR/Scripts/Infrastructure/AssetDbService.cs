using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common;
using UniRx;
using UnityEditor;
using UnityEngine;
using Zenject;
using CancellationToken = System.Threading.CancellationToken;

namespace RR.Scripts.Infrastructure
{
    public interface IAssetDbService// : IDisposable
    {
        IList<Sprite> GetCards();
        IList<string> GetCardsNameList();
        Sprite GetCardByName(string cardName);

        void OnDestroy();

        void Init();
        ReactiveProperty<bool> Initilized { get; set; }
    }

    public sealed class AssetDbService : IAssetDbService
    {
        private IWebContentService    _webService = default;
        private IResourceCacheService _cacheService   = default;

        private string _pathToCardsArt;
        private CancellationToken _token;

        public ReactiveProperty<bool> Initilized { get; set; } = new ReactiveProperty<bool>(false);

        [Inject]
        public void Ctor(
            IWebContentService webService, IResourceCacheService cacheService,
            string pathToCardsArt, CancellationToken token)
        {
            _webService     = webService;
            _cacheService   = cacheService;
            _pathToCardsArt = pathToCardsArt;

            _token = token;
        }

        /// <param name="quality"> if 100 save texture to png, else to jpg with selected quality</param>
        public async void Init()
        {
            var cardsId = new List<string>();
            
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var texture = await _webService.LoadTexture("https://picsum.photos", 180, 180, _token);
                    _token.ThrowIfCancellationRequested();
            
                    int quality = 100;
                    var img = quality == 100 ? texture.EncodeToPNG() : texture.EncodeToJPG(quality);
                    var extension = quality == 100 ? "png" : "jpg";
            
                    cardsId.Add(texture.name);
                    
                    var filename = $"{_pathToCardsArt}/{texture.name}.{extension}";
            
                    using (var fs = File.Open(filename, FileMode.Create))
                    {
                        await fs.WriteAsync(img, 0, img.Length, _token);
                    }
            
                    await Task.Delay(300, _token); // limit the number of requests per second
                }
                catch (OperationCanceledException e)
                {
                    Debug.Log("Resource download has been canceled");
                }
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh(); // freezy Editor
#endif     
            foreach (var id in cardsId)
            {
#if UNITY_EDITOR
                var asset = _cacheService.Load<Sprite>($"CardsArt/{id}");
#else
                // todo Load From persistentDataPath
#endif
            }

            Initilized.Value = true;
        }

        public Sprite GetCardByName(string cardName)
        {
            return _cacheService.Load<Sprite>(cardName);
        }

        public IList<Sprite> GetCards()
        {
            return _cacheService.LoadAll<Sprite>(_cacheService.CachePaths);
        }

        public IList<string> GetCardsNameList()
        {
            return _cacheService.CachePaths;
        }

        public void OnDestroy()
        {
            _cacheService.UnloadAll();
            
            Directory.Delete(_pathToCardsArt, true);
            AssetDatabase.Refresh();
        }
    }
}
