using System.IO;
using System.Threading;
using Common;
using RR.Scripts.Infrastructure;
using RR.Scripts.Models;
using RR.Scripts.Presenters;
using UniRx;
using UnityEngine;
using Zenject;

namespace RR.Scripts
{
    public class Startup : MonoInstaller, IInitializable
    {
        public Canvas Canvas;
        public GameObject Hand;

        public GameObject CardPrefab;
        

        public string relativePathToCardsArt = "CardsArt";
        

        private string _pathToCardsArt;

        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public override void InstallBindings()
        {
#if UNITY_EDITOR
            _pathToCardsArt = $"{Application.dataPath}/Resources/{relativePathToCardsArt}";
#else
            pathToCardsArt = $"{Application.persistentDataPath}/{relativePathToCardsArt}";
#endif
            Directory.CreateDirectory(_pathToCardsArt);
            Container.BindInterfacesTo<Startup>().FromInstance(this);
            
            InstallSpriteDb();

            Container.Bind<HandPresenter>().FromComponentOn(Hand).AsSingle();


            // Container.BindFactory<int,int,int, string, string, Sprite, CardModel, CardModel.Factory>().AsSingle();
            // Container.BindFactory<IReactiveCollection<CardModel>, HandModel, HandModel.Factory>().AsSingle();
            //
            // Container.BindFactory<UnityEngine.Object, CardModel, CardPresenter, CardPresenter.Factory>().AsSingle();
            //
            //Container.Bind<HandModel>().AsSingle();
            // Container.Bind<ReactiveCollection<CardModel>>().AsSingle();

            // Container.Bind<CardModel>().AsTransient();
            // Container.BindFactory<CardPresenter, CardPresenter.Factory>()
            //     .FromComponentInNewPrefab(CardPrefab)
            //     .WithGameObjectName("Card")
            //     .UnderTransform(Canvas.transform)
            //     .AsTransient();

            // Container.Bind<HandPresenter>().AsSingle();
        }
        
        private void InstallSpriteDb()
        {
            Container.Bind<IWebContentService>().To<WebContentService>().AsSingle();
            // equivalent 
            Container.BindInterfacesAndSelfTo<ResourceCacheService>().AsSingle();
            
            //Container.Bind<AssetDbService>().FromComponentInNewPrefab(assetDbServicePrefab).AsSingle().WithArguments(_pathToCardsArt, _source.Token).NonLazy();
            //Container.BindInterfacesAndSelfTo<AssetDbService>().FromComponentInNewPrefab(assetDbServicePrefab).AsSingle().WithArguments(_pathToCardsArt, _source.Token).NonLazy();
            
            Container.BindInterfacesAndSelfTo<AssetDbService>().AsSingle().WithArguments(_pathToCardsArt, _source.Token).NonLazy();
        }


        public void Initialize()
        {
            InitializeAssetDb();
            InitializeHand();
        }
        
        private void InitializeAssetDb()
        {
            Container.Resolve<IAssetDbService>().Init();
        }
        
        private void InitializeHand()
        {
            Container.Resolve<IAssetDbService>().Initilized
                .ObserveEveryValueChanged(x => x.Value)
                .Where(x => x)
                .Subscribe(xs => InitializingHand()).AddTo(this);
        }

        private void InitializingHand()
        {
            Debug.Log("db init: " + Container.Resolve<IAssetDbService>().GetCardsNameList().Count);

            Container.Resolve<HandPresenter>().Init();

            //var factory = Container.Resolve<CardPresenter.Factory>();
            //factory.Create();

            //Container.InstantiatePrefab()

            //Container.InstantiatePrefabResourceForComponent<CardPresenter>("Card");

            //Container.InstantiatePrefabForComponent<CardPresenter>(CardPrefab);

            //Container.InstantiateComponent<CardPresenter>(CardPrefab);

            // var cardModelFactory = Container.Resolve<CardModelFactory>();
            // var cardPresenterFactrory = Container.Resolve<CardPresenterFactrory>();
            // var sprites = Container.Resolve<IAssetDbService>().GetCards();
            //
            // var cardPrefab = Resources.Load("Card") as GameObject;
            //
            // //var hand = Container.InstantiatePrefabForComponent(Container.Resolve<HandPresenter>());
            //
            // IReactiveCollection<CardPresenter> cards = new ReactiveCollection<CardPresenter>();
            //
            // for (int i = 0; i < 10; i++)
            // {
            //     var card = cardModelFactory.Create(9, 9, 9, "title",  "desc", sprites[i]);
            //     //Container.Inject(card);
            //     var presenter = cardPresenterFactrory.Create(cardPrefab, card);
            //     //cards.Add(presenter);
            // }
        }
        
        private void OnDestroy()
        {
            _source.Cancel();
            Container.Resolve<IAssetDbService>().OnDestroy();
        }
    }
}