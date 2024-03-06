using UnityEngine;
using GameEngine.Library.Utils;

namespace GameEngine.Game.Core
{
    public class PoolManager : PersistentSingleton<PoolManager>
    {
        [SerializeField] private SimpleObjectPooler _productionMenuItemPooler;

        public PoolableObject GetProductionMenuItem()
        {
			return _productionMenuItemPooler.GetPooledGameObject();
		}
    }
}