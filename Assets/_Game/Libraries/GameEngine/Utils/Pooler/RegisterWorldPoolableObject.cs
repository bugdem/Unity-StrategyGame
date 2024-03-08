using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Library.Utils
{
    [RequireComponent(typeof(PoolableObject))]
	[DefaultExecutionOrder(1)]
    public class RegisterWorldPoolableObject : MonoBehaviour
    {
		[SerializeField] protected SimpleObjectPooler _pooler;

		protected PoolableObject _poolableObject;

		protected virtual void Awake()
		{
			_poolableObject = GetComponent<PoolableObject>();
			_pooler.RegisterPoolableObjectInWorld(_poolableObject);
		}
	}
}