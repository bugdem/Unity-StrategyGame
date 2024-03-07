using UnityEngine;
using GameEngine.Library.Utils;

namespace GameEngine.Game.Core
{
    public class PoolManager : PersistentSingleton<PoolManager>
    {
        [SerializeField] private SimpleObjectPooler _menuItemProductionPooler;
        [SerializeField] private SimpleObjectPooler _boardProductionPooler;

        public PoolableObject GetMenuItemForProduction()
        {
			return _menuItemProductionPooler.GetPooledGameObject();
		}

        public PoolableObject GetBoardProduction()
        {
            return _boardProductionPooler.GetPooledGameObject();
        }
    }
}