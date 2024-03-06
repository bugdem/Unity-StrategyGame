using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class BoardGrid : MonoBehaviour
    {
        [SerializeField] private Grid _grid;

        public Grid Grid => _grid;
    }
}