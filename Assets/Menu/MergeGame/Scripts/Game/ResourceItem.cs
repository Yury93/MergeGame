using System;
 
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MergeGame
{

    [Serializable]
    public class ResourceItem : MonoBehaviour, ILoaderProgress, IResource
    { 
        [SerializeField] protected TextMeshProUGUI scoreText;
        public int count;
        [field: SerializeField] public Image icon { get;protected set; }
        public virtual Type ResourceType => typeof(ResourceItem);
        public Action<ResourceItem> onChangeResource;
        public virtual void ShowScore() =>
             scoreText.text = count.ToString();
        public virtual bool HasResource(int count)
        {
            if (count > this.count) return false;
            else return true;
        }
        public virtual void TakeResource(int count)
        {
            if (HasResource(count) == false) Debug.LogError("no resources");
            this.count -= count;
            ShowScore();
            onChangeResource?.Invoke(this);
        }
        public virtual void AddResource(int count)
        {
            this.count += count;
            ShowScore();
            onChangeResource?.Invoke(this);
        }

        public virtual void Load(PlayerProgress progress)
        {
            var resourceData = progress.resources.FirstOrDefault(r => r.ResourceType == ResourceType);
            if (resourceData != null)
               count = resourceData.Count ;

            ShowScore();
        }
    }
}