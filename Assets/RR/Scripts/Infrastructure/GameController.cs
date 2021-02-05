using RR.Scripts.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RR.Scripts.Infrastructure
{
    public class GameController : MonoBehaviour
    {
        public Button RndSetter;

        private HandPresenter _handPresenter;

        private IAssetDbService _assetDb;
        
        

        [Inject]
        public void Ctor(IAssetDbService assetDb)
        {
            _assetDb = assetDb;
            
            
        }
    }
}
