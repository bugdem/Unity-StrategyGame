using UnityEngine;
using UnityEngine.SceneManagement;
using GameEngine.Library.Utils;

namespace GameEngine.Game.Core
{
	// Exposes TouchControls to be used in other classes.
	// Button statuses can be accessed directly or with event listeners.
	[DefaultExecutionOrder(-1)]
	public class InputManager : PersistentSingleton<InputManager>
	{
		public static InputButton TouchButton { get; private set; }
		public static InputButton MoveToPositionButton { get; private set; }
		public static Vector2 TouchPosition { get; private set; }
		public static Vector2 MoveToPosition { get; private set; }
		public static Vector2 PointerPosition { get; private set; }
		public static Vector2 TouchStartPosition { get; private set; }
		public static Vector2 TouchEndPosition { get; private set; }

		private TouchControls _touchControls;

		protected override void Awake()
		{
			base.Awake();
			if (!_enabled) return;

			_touchControls = new TouchControls();

			TouchButton = new InputButton();
			MoveToPositionButton = new InputButton();
		}

		private void Update()
		{
			TouchButton.Update();
			MoveToPositionButton.Update();
			TouchPosition = _touchControls.Player.TouchPosition.ReadValue<Vector2>();
		}

		private void Start()
		{
			_touchControls.Player.Touch.started += OnTouchStarted;
			_touchControls.Player.Touch.canceled += OnTouchCanceled;
			_touchControls.Player.MoveToPosition.started += OnMoveToPositionStarted;
			_touchControls.Player.MoveToPosition.canceled += OnMoveToPositionCanceled;
		}

		private void OnTouchStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			// Debug.Log("Touch Started!");

			TouchStartPosition = _touchControls.Player.TouchPosition.ReadValue<Vector2>();
			TouchPosition = TouchStartPosition;
			TouchButton.OnTouchStarted();
		}

		private void OnTouchCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			// Debug.Log("Touch Ended!");

			TouchEndPosition = _touchControls.Player.TouchPosition.ReadValue<Vector2>();
			TouchButton.OnTouchCanceled();
		}

		private void OnMoveToPositionStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			// Debug.Log("Move To Position Started!");

			MoveToPosition = _touchControls.Player.TouchPosition.ReadValue<Vector2>();
			MoveToPositionButton.OnTouchStarted();
		}

		private void OnMoveToPositionCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			// Debug.Log("Move To Position Ended!");

			MoveToPositionButton.OnTouchCanceled();
		}

		private void ResetValues()
		{
			TouchButton.Reset();
			MoveToPositionButton.Reset();

			TouchPosition = Vector2.zero;
			TouchStartPosition = Vector2.zero;
			MoveToPosition = Vector2.zero;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			ResetValues();
		}

		private void OnEnable()
		{
			_touchControls.Enable();

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDisable()
		{
			_touchControls.Disable();

			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	// Handles input button to be accessed by anytime in execution.
	// Button status can be checked with IsDown in Update method or via OnTouchDown Event.
	public class InputButton
	{
		public bool IsTouchDown { get; private set; }
		public bool IsTouchUp { get; private set; }
		public bool WasTouching { get; private set; }
		public bool IsTouching { get; private set; }

		public delegate void TouchHandler();
		public delegate void TouchHandlerStatus(bool status);

		public event TouchHandler OnTouchDown;
		public event TouchHandler OnTouchUp;

		public void Update()
		{
			IsTouchDown = false;
			IsTouchUp = false;

			if (WasTouching && !IsTouching)
			{
				WasTouching = IsTouching;
				IsTouchUp = true;
			}
			else if (!WasTouching && IsTouching)
			{
				WasTouching = IsTouching;
				IsTouchDown = true;
			}
		}

		public void Reset()
		{
			IsTouchDown = false;
			IsTouchUp = false;
			WasTouching = false;
			IsTouching = false;
			OnTouchDown = null;
			OnTouchUp = null;
		}

		public void OnTouchStarted()
		{
			IsTouching = true;
			OnTouchDown?.Invoke();
		}

		public void OnTouchCanceled()
		{
			IsTouching = false;
			OnTouchUp?.Invoke();
		}
	}
}