using System;
using System.Linq;
using DG.Tweening;
using RR.Scripts.Common;
using RR.Scripts.Extension;
using RR.Scripts.Infrastructure;
using RR.Scripts.Models;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Math = RR.Scripts.Extension.Math;

namespace RR.Scripts.Presenters
{
    public class HandPresenterFactory : PlaceholderFactory<ReactiveCollection<CardPresenter>, HandPresenter>
    {
    }
    
    public class HandPresenter : MonoBehaviour
    {
        public Button RandomSetter;
        public RectTransform Arc3Root;
        
        private IReactiveCollection<CardPresenter> _cards;
        private HandModel _model;

        private int _id = 0;
        private Arc3 _arc;

        private IAssetDbService _assetDbService;

        [Inject]
        public void Ctor(IAssetDbService assetDbService)
        {
            _assetDbService = assetDbService;
        }
        
        // public void Ctor(HandModel handModel)
        // {
        //     _model = handModel;
        // }

        public void Init()
        {
            _cards = new ReactiveCollection<CardPresenter>();

            RandomSetter.interactable = true;
            
            RandomSetter
                .OnClickAsObservable()
                .Subscribe(_ => _cards[_id++ % _cards.Count].SetRandomValue())
                .AddTo(this);
            
            _cards
                .ObserveAdd()
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(c => RebuildCardPosition())
                .AddTo(this);
            
            _cards
                .ObserveRemove()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(c => RebuildCardPosition())
                .AddTo(this);
            
            _cards
                .ObserveAdd()
                .Subscribe(c => AddDragDrop(c.Value))
                .AddTo(this);

            foreach (var cc in FindObjectsOfType<CardPresenter>())
            {
                var cardModel = new CardModel(_assetDbService);
                cc.Ctor(cardModel);
                
                _cards.Add(cc);
            }

            _arc = new Arc3() 
            { 
                p0 = Arc3Root.gameObject.transform.InverseTransformPoint(Arc3Root.GetChild(0).transform.position), 
                p1 = Arc3Root.gameObject.transform.InverseTransformPoint(Arc3Root.GetChild(1).transform.position), 
                p2 = Arc3Root.gameObject.transform.InverseTransformPoint(Arc3Root.GetChild(2).transform.position),
            };

            // _model.IsDead
            //     .Where(isDead => isDead)
            //     .Subscribe(_ => 
            //     {
            //         _model = null;
            //         Destroy(gameObject);
            //     })
            //     .AddTo(this);;
            //
            // AddDragDrop(this);
        }

        private void ExtractCard(CardPresenter cardPresenter)
        {
            var res = _cards.Remove(cardPresenter);
            //Debug.Log(res);
            //_id--;
        }

        private void ReturnCard(CardPresenter cardPresenter)
        {
            _cards.Add(cardPresenter);
            //_id++;
        }

        private void AddDragDrop(CardPresenter cardPresenter)
        {
            var cardGo = cardPresenter.gameObject;
            // var cardTr = cardPresenter.transform;
            
            cardGo
                .EnsureGetComponent<ObservableBeginDragTrigger>()
                .OnBeginDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => Begin(cardPresenter))
                .AddTo(cardPresenter);

            cardGo
                .EnsureGetComponent<ObservableDragTrigger>()
                .OnDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => Drag(cardPresenter, v))
                .AddTo(cardPresenter);

            cardGo
                .EnsureGetComponent<ObservableEndDragTrigger>()
                .OnEndDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => End(cardPresenter))
                .AddTo(cardPresenter);
        }
        
        private void Begin(CardPresenter cardPresenter)
        {
            cardPresenter.transform.DOScale(Vector3.one * 2.5f, 0.15f);
            //ExtractCard(cardPresenter); // not work
        }

        private void Drag(CardPresenter cardPresenter, Vector3 mousePosition)
        {
            cardPresenter.transform.position = mousePosition;
        }

        private void End(CardPresenter cardPresenter)
        {
            cardPresenter.transform.DOKill();
            cardPresenter.transform.DOScale(Vector3.one, 0.15f);

            //ReturnCard(cardPresenter);
        }

        private void RebuildCardPosition()
        {
            float step = 1.0f / _cards.Count;
            int cardIndex = 0;
            
            var offsetCenter = _arc.p1 + Vector3.down * 2000;

            var sortedCard = _cards.OrderBy(card => card.transform.position.x).ToList();
            
            sortedCard.ForEach(card => card.DOKill());

            for (float t = 0; t < 1 ; t += step, cardIndex++)
            {
                var point = Math.CalculateCubicBezierPoint(t, _arc);
                var angle = Vector3.Angle(point + offsetCenter, Vector3.down);
                
                if (t > 0.5f) angle = -angle;
                
                // Debug.DrawLine(O, point, Color.red, 10);
                // Debug.DrawRay(O, Vector3.left * 1000, Color.green, 10);
                // Debug.Log("point " + point + " angle " + angle);
                
                point = Arc3Root.TransformPoint(point);
                
                var lDirection = new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));
                
                sortedCard[cardIndex].transform.DOMove(point, 0.25f);
                sortedCard[cardIndex].transform.DORotate(lDirection, 0.25f);
                
                // _cards[cardIndex].transform.position = point;
                // _cards[cardIndex].transform.Rotate(Vector3.forward, angle);
            }
        }
    }
}
