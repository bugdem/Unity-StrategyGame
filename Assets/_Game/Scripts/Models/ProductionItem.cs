using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ProductionItem : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _menuItemIcon;

        public string Name => _name;
        public Sprite MenuItemIcon => _menuItemIcon;
    }
}