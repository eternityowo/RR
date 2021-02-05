using System;
using DG.Tweening;
using RR.Scripts.Extension;
using RR.Scripts.Models;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace RR.Scripts.Presenters
{
    public class CardPresenter : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<CardPresenter>
        {}
        
        private CardModel _model;

        // view
        public TextMeshProUGUI Hp;
        public TextMeshProUGUI Mana;
        public TextMeshProUGUI Damage;

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Desc;

        public Image CardArt;

        //[Inject]
        public void Ctor(CardModel cardModel)
        {
            _model = cardModel;
            
            CounterAnimation(_model.Hp).SubscribeToText(Hp).AddTo(this);
            CounterAnimation(_model.Mana).SubscribeToText(Mana).AddTo(this);
            CounterAnimation(_model.Damage).SubscribeToText(Damage).AddTo(this);

            _model.Title.SubscribeToText(Title).AddTo(this);
            _model.Desc.SubscribeToText(Desc).AddTo(this);

            _model.CardArt.SubscribeToImage(CardArt).AddTo(this);
        }

        public void SetRandomValue()
        {
            var rndIndex = Random.Range(0, Enum.GetNames(typeof(CardParamType)).Length);
            Enum.TryParse(rndIndex.ToString(), out CardParamType type);

            var value = Random.Range(-2, 10);
            
            SetValue(type, value);
        }

        public void SetValue(CardParamType type, int value)
        {
            switch (type)
            {
                case CardParamType.Hp:
                    _model.Hp.Value = value;
                    break;
                case CardParamType.Mana:
                    _model.Mana.Value = value;
                    break;
                case CardParamType.Damage:
                    _model.Damage.Value = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private UniRx.IObservable<int> CounterAnimation(IReactiveProperty<int> property, TimeSpan delay = default)
        {
            return property
                .Skip(1)
                .Scan(new {Previous = 0, Current = property.Value},
                    (accumulator, newVal) => new {Previous = accumulator.Current, Current = newVal})
                .Select(pair =>
                {
                    var direction = pair.Current > pair.Previous ? 1 : -1;
                    return Observable.Interval(delay == default ? TimeSpan.FromMilliseconds(30) : delay)
                        .Scan(pair.Previous, (accumulator, _) => accumulator + direction)
                        .TakeWhile(val => val != pair.Current + direction);
                })
                .Switch()
                .StartWith(property.Value);
        }
    }
}
