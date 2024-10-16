using Menu.DragItems;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

namespace Menu.MergeGame
{
    public class MergeItem : DragItem
    { 
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private float accumulatedTime;
        [SerializeField] public int Level { get; private set; }
        [field: SerializeField] public List<Image> Images { get; private set; }
        public MergeItemEntity ItemEntity { get; private set; }
        public float AccumulatedResource { get; set; } 
        public bool IsDragProcess { get; private set; }

        public Action onBeginDrag;
        public Action onEndDrag;
 
        public void Init(MergeItemEntity itemEntity, int startPower)
        {
            this.ItemEntity = itemEntity;
            Level = startPower;
            Images.ForEach(i => i.sprite = itemEntity.sprite);
            powerText.text = Level.ToString();
            accumulatedTime = Time.time + 1;
        } 
        public bool WaitSecond()
        {
            if (Time.time > accumulatedTime)
            {
                accumulatedTime = Time.time + 1;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void Updated()
        {
            if(WaitSecond())
            {
                AddAccumulatedResource();
            }
        }
        private void AddAccumulatedResource() =>
         AccumulatedResource += ItemEntity.energy_gen;
        public float ClaimResource()
        {
            float resource = AccumulatedResource;
            AccumulatedResource = 0;
            return resource;
        }
      

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            IsDragProcess = true;
            onBeginDrag?.Invoke();
        }
        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            IsDragProcess = true;
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            IsDragProcess = false;
            onEndDrag?.Invoke();
        }
        public void PowerUp()
        {
            Level += 1;
            powerText.text = Level.ToString();
        }
    }
}