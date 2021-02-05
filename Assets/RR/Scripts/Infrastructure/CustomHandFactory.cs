using System.Collections.Generic;
using RR.Scripts.Models;
using RR.Scripts.Presenters;
using UnityEngine;
using Zenject;

namespace RR.Scripts.Infrastructure
{
    public interface ICustomHandFactory
    {
        void Create();
        void Load();
    }
    
    public class CustomHandFactory : IFactory<HandPresenter>
    {
        private const string CardPrefabPath = "Card";

        private GameObject _cardPrefab;

        private IList<Sprite> cardsArt;

        private readonly DiContainer _diContainer;

        public CustomHandFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public HandPresenter Create()
        {
            //_diContainer.InstantiatePrefab(_meleeEnemyPrefab, at, Quaternion.identity, null);
            foreach (var sprite in cardsArt)
            {
                _diContainer.InstantiatePrefab(_cardPrefab, Vector3.zero,  Quaternion.identity, null);
            }

            return null;
        }

        public void Load()
        {
            cardsArt = _diContainer.Resolve<IAssetDbService>().GetCards();
            _cardPrefab = Resources.Load(CardPrefabPath) as GameObject;
            
            Debug.Log(cardsArt.Count);
            Debug.Log(_cardPrefab != null);
        }
    }
}