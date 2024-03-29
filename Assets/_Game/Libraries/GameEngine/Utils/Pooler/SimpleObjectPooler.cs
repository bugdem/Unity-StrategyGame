﻿// Extension of Corgi Engine's MMSimpleObjectPooler.
// Credits to More Mountain.

using UnityEngine;
using System.Collections.Generic;

namespace GameEngine.Library.Utils
{
	/// <summary>
	/// A simple object pool outputting a single type of objects
	/// </summary>
	public class SimpleObjectPooler : ObjectPooler
	{
		/// the game object we'll instantiate 
		public PoolableObject GameObjectToPool;
		/// the number of objects we'll add to the pool
		public int PoolSize = 20;
		/// if true, the pool will automatically add objects to the itself if needed
		public bool PoolCanExpand = true;

		/// the actual object pool
		protected List<PoolableObject> _pooledGameObjects;

		/// <summary>
		/// Fills the object pool with the gameobject type you've specified in the inspector
		/// </summary>
		public override void FillObjectPool()
		{
			if (GameObjectToPool == null)
			{
				return;
			}

			CreateWaitingPool();

			// we initialize the list we'll use to 
			_pooledGameObjects = new List<PoolableObject>();

			int objectsToSpawn = PoolSize;

			if (_objectPool != null)
			{
				objectsToSpawn -= _objectPool.PooledGameObjects.Count;
				_pooledGameObjects = new List<PoolableObject>(_objectPool.PooledGameObjects);
			}

			// we add to the pool the specified number of objects
			for (int i = 0; i < objectsToSpawn; i++)
			{
				AddOneObjectToThePool();
			}
		}

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected override string DetermineObjectPoolName()
		{
			return ("[SimpleObjectPooler] " + this.name);
		}

		/// <summary>
		/// This method returns one inactive object from the pool
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override PoolableObject GetPooledGameObject()
		{
			// we go through the pool looking for an inactive object
			for (int i = 0; i < _pooledGameObjects.Count; i++)
			{
				if (!_pooledGameObjects[i].gameObject.activeInHierarchy)
				{
					// if we find one, we return it
					return _pooledGameObjects[i];
				}
			}
			// if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
			if (PoolCanExpand)
			{
				return AddOneObjectToThePool();
			}
			// if the pool is empty and can't grow, we return nothing.
			return null;
		}

		/// <summary>
		/// Adds one object of the specified type (in the inspector) to the pool.
		/// </summary>
		/// <returns>The one object to the pool.</returns>
		protected virtual PoolableObject AddOneObjectToThePool()
		{
			if (GameObjectToPool == null)
			{
				Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
				return null;
			}
			var newGameObject = Instantiate(GameObjectToPool);
			newGameObject.gameObject.SetActive(false);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_waitingPool.transform);
			}
			newGameObject.name = GameObjectToPool.name + "-" + _pooledGameObjects.Count;

			_pooledGameObjects.Add(newGameObject);

			_objectPool.PooledGameObjects.Add(newGameObject);

			ObjectAdded(newGameObject);

			return newGameObject;
		}

		public virtual void RegisterPoolableObjectInWorld(PoolableObject poolableObject)
		{
			if (NestWaitingPool)
				poolableObject.transform.SetParent(_waitingPool.transform);

			_pooledGameObjects.Add(poolableObject);
			_objectPool.PooledGameObjects.Add(poolableObject);

			ObjectAdded(poolableObject);
		}

		/// <summary>
		/// Gets poolable objects from pool to be used on scene unload event, which destroys used poolable objects.
		/// </summary>
		/// <returns>The pool objects.</returns>
		protected override List<PoolableObject> GetPoolObjects()
		{
			return _pooledGameObjects;
		}
	}
}