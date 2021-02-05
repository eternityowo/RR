using UniRx;
using UnityEngine;
using Zenject;

namespace RR.Scripts.Models
{
    public class HandModel
    {
        public class HandFactory : PlaceholderFactory<IReactiveCollection<CardModel>, HandModel> { }
        
        public IReactiveCollection<CardModel> Cards { get; private set; }
        
        public HandModel(IReactiveCollection<CardModel> cards)
        {
            Cards = cards;
        }

        public HandModel()
        {
            Cards = new ReactiveCollection<CardModel>();
        }

        public void Add(CardModel cardModel)
        {
            Cards.Add(cardModel);
        }
        
        public bool Delete(CardModel cardModel)
        {
            return Cards.Remove(cardModel);
        }
    }
    
}
