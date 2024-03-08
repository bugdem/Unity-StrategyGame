using UnityEngine;
using GameEngine.Library.Utils;

namespace GameEngine.Game.Core
{
    public class PoolManager : PersistentSingleton<PoolManager>
    {
		[SerializeField] private SimpleObjectPooler _boardProductionPooler;
		[SerializeField] private SimpleObjectPooler _boardProductionItemPooler;
		[SerializeField] private SimpleObjectPooler _menuItemProductionPooler;
        [SerializeField] private SimpleObjectPooler _menuItemProductionItemPooler;

		public PoolableObject GetBoardProduction()
		{
			return _boardProductionPooler.GetPooledGameObject();
		}

		public PoolableObject GetBoardProductionItem()
		{
			return _boardProductionItemPooler.GetPooledGameObject();
		}

		public PoolableObject GetMenuItemForProduction()
        {
			return _menuItemProductionPooler.GetPooledGameObject();
		}

		public PoolableObject GetMenuItemForProductionItem()
		{
			return _menuItemProductionItemPooler.GetPooledGameObject();
		}
	}
}