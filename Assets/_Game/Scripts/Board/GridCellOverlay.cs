using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class GridCellOverlay : PoolableObject
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _defeaultColor = Color.white;
        [SerializeField] private Color _availableColor = Color.green;
        [SerializeField] private Color _notAvailableColor = Color.red;

        public Vector3Int CellIndex { get; set; }

        public void SetDefaultColor()
        {
            SetColor(_defeaultColor);
        }

        public void SetAvailableColor()
        {
			SetColor(_availableColor);
		}

        public void SetNotAvailableColor()
        {
            SetColor(_notAvailableColor);
        }

        private void SetColor(Color color)
        {
			_spriteRenderer.color = color;
		}
    }
}