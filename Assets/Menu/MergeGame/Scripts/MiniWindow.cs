using UnityEngine;
using UnityEngine.UI;

namespace Menu.MergeGame
{
    public class MiniWindow : MonoBehaviour
    {
        [SerializeField] protected GameObject window;
   
        public bool IsOpen { get; private set; }
        
        public virtual void Open()
        {
            window.SetActive(true);
            IsOpen = true;
        }
        public virtual void Close()
        {
            window.SetActive(false);
            IsOpen = false;
        }
    }
}