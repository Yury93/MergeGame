
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Menu.MergeGame
{
    [Serializable] 
    public class WindowService 
    { 
        [field: SerializeField] public List<MiniWindow> Windows { get; private set; }
        public RuneShop ShopWindow { get; private set; }
        public SpineWindow SpineWindow { get; private set; }
        MergeGameController gameController;
        public void Init(MergeGameController gameController)
        {
            this.gameController = gameController;

            ShopWindow = GetWindow<RuneShop>();
            ShopWindow.Init(gameController);
            SpineWindow = GetWindow<SpineWindow>();
            SpineWindow.Init(gameController);
        }

        public void OpenWindow<T>() where T : MiniWindow
        {
            GetWindow<T>().Open();
        }
        public void CloseWindow<T>() where T : MiniWindow
        {
            GetWindow<T>().Close();
        }

        public T GetWindow<T>()
        {
          return Windows.OfType<T>().FirstOrDefault();
        }
    }
}