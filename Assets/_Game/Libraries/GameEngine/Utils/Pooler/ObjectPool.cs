// Extension of Corgi Engine's MMObjectPool. 
// Credits to More Mountain.

using UnityEngine;
using System.Collections.Generic;
using GameEngine.Library.Editor;

namespace GameEngine.Library.Utils
{
    public class ObjectPool : MonoBehaviour
    {
        [ReadOnly]
        public List<PoolableObject> PooledGameObjects;
    }
}
