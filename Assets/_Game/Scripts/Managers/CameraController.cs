using Cinemachine;
using DG.Tweening;
using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private Camera _mainCamera;
		[SerializeField] private CinemachineVirtualCameraBase _cmMain;

		[Header("Bounds")]
		[SerializeField] private Vector2 _bounds = new Vector2(10, 10);
		[SerializeField] private Vector2 _boundsOffset = new Vector2(0, 0);

		[Header("Focus")]
		[SerializeField] private Ease _focusEase = Ease.OutCubic;
		[SerializeField] private float _focusDuration = 0.5f;
		[SerializeField] private Vector3 _focusOffset;

		[Header("Pan")]
		[SerializeField] private float _panSpeed = 1f;

        public Camera MainCamera => _mainCamera;
		public bool IsPanning { get; private set; }

		private Tweener _focusTween;
		private Vector2 _previousTouchPosition;

		private void LateUpdate()
		{
			if (IsPanning)
			{
				Vector3 touchDelta = InputManager.TouchPosition - _previousTouchPosition;
				Vector3 newPosition = _cmMain.transform.position - touchDelta;
				newPosition = Vector3.MoveTowards(_cmMain.transform.position, newPosition, _panSpeed * Time.deltaTime);
				newPosition = ClampToBounds(newPosition);
				_cmMain.transform.position = newPosition;

				_previousTouchPosition = InputManager.TouchPosition;
			}
		}

		public void StartPan()
		{
			IsPanning = true;
			StopFocus();

			_previousTouchPosition = InputManager.TouchPosition;
		}

		public void StopPan()
		{
			IsPanning = false;
		}

		public void Focus(Vector3 position)
		{
			StopFocus();
			StopPan();

			Vector3 targetPosition = new Vector3(position.x, position.y, _cmMain.transform.position.z);
			targetPosition = ClampToBounds(targetPosition + _focusOffset);
			_focusTween = _cmMain.transform.DOMove(targetPosition, _focusDuration)
												.SetEase(_focusEase)
												.OnComplete(() =>
												{
													_focusTween = null;
												});
		}

		public void StopFocus()
		{
			if (_focusTween != null)
			{
				_focusTween.Kill();
				_focusTween = null;
			}
		}

		private Vector3 ClampToBounds(Vector3 position)
		{
			float x = position.x.Clamp(_boundsOffset.x - _bounds.x * .5f, _boundsOffset.x + _bounds.x * .5f);
			float y = position.y.Clamp(_boundsOffset.y - _bounds.y * .5f, _boundsOffset.y + _bounds.y * .5f);
			return new Vector3(x, y, 0f);
		}

		private void OnDrawGizmos()
		{
			GEDebug.DrawGizmoRectangle(_boundsOffset, _bounds, Color.red);
		}
	}
}