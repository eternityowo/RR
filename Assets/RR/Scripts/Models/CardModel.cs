using System.Linq;
using RR.Scripts.Infrastructure;
using UniRx;
using UnityEngine;
using Zenject;

namespace RR.Scripts.Models
{
    public class CardModel
    {
        public ReactiveProperty<int> Hp { get; private set; }
        public ReactiveProperty<int> Mana { get; private set; }
        public ReactiveProperty<int> Damage { get; private set; }

        public ReactiveProperty<string> Title { get; private set; }

        public ReactiveProperty<string> Desc { get; private set; }

        public ReactiveProperty<Sprite> CardArt { get; private set; }
        public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }


        public class Factory : PlaceholderFactory<int, int, int, string, string, Sprite, CardModel> { }

        //[Inject]
        public CardModel(int hp, int mana, int damage, string title, string desc, Sprite sprite)
        {
            Hp = new ReactiveProperty<int>(hp);
            Mana = new ReactiveProperty<int>(mana);
            Damage = new ReactiveProperty<int>(damage);

            IsDead = Hp.Select(x => x <= 0).ToReactiveProperty();

            Title = new ReactiveProperty<string>(title);
            Desc = new ReactiveProperty<string>(desc);

            CardArt = new ReactiveProperty<Sprite>(sprite);
        }

        //[Inject]
        public CardModel(IAssetDbService dbService)
        {
            Hp = new ReactiveProperty<int>(Random.Range(1, 9));
            Mana = new ReactiveProperty<int>(Random.Range(1, 9));
            Damage = new ReactiveProperty<int>(Random.Range(1, 9));

            IsDead = Hp.Select(x => x <= 0).ToReactiveProperty();

            Title = new ReactiveProperty<string>("Title");
            Desc = new ReactiveProperty<string>("Description");

            var cards = dbService.GetCards();
            
            CardArt = new ReactiveProperty<Sprite>(cards[Random.Range(0, cards.Count)]); 
        }
    }
}
