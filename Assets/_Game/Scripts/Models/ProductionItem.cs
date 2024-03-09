using System;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public abstract class ProductionItem : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _menuItemIcon;

        public string Name => _name;
        public Sprite MenuItemIcon => _menuItemIcon;
    }
}