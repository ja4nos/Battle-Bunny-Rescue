using JetBrains.Annotations;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;
using UnityEngine.UIElements;

namespace Project.Input
{
	[UsedImplicitly]
	[Serializable]
	public class HoldTriggerInteraction : IInputInteraction
	{
		[RuntimeInitializeOnLoadMethod]
		public static void Init()
		{
			InputSystem.RegisterInteraction<HoldTriggerInteraction>();
		}

		/// <summary>
		/// Duration in seconds that the control must be pressed for the hold to register.
		/// </summary>
		/// <remarks>
		/// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultHoldTime" /> is used.
		/// Duration is expressed in real time and measured against the timestamps of input events
		/// (LowLevel.InputEvent.time) not against game time (Time.time).
		/// </remarks>
		public float Duration;

		public float HoldInterval = 0.2f;
		public float HoldIntervalMin = 0.1f;
		public float HoldIntervalSpeedUpScale = 0.9f;

		/// <summary>
		/// Magnitude threshold that must be crossed by an actuated control for the control to
		/// be considered pressed.
		/// </summary>
		/// <remarks>
		/// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint" /> is used instead.
		/// </remarks>
		/// <seealso cref="InputControl.EvaluateMagnitude()" />
		public float PressPoint;

		private float DurationOrDefault => Duration > 0.0 ? Duration : InputSystem.settings.defaultHoldTime;
		private float PressPointOrDefault => PressPoint > 0.0 ? PressPoint : InputSystem.settings.defaultButtonPressPoint;

		private double _timePressed;
		private float _currentHoldInterval;

		public void Process(ref InputInteractionContext context)
		{
			if(context.timerHasExpired)
			{
				context.PerformedAndStayPerformed();
				_currentHoldInterval = Math.Max(_currentHoldInterval * HoldIntervalSpeedUpScale, HoldIntervalMin);
				context.SetTimeout(_currentHoldInterval);
				return;
			}

			switch(context.phase)
			{
				case InputActionPhase.Waiting:
					if(context.ControlIsActuated(PressPointOrDefault))
					{
						_timePressed = context.time;

						context.Started();
						context.SetTimeout(DurationOrDefault);
					}

					break;

				case InputActionPhase.Started:
					// If we've reached our hold time threshold, perform the hold.
					// We do this regardless of what state the control changed to.
					if(context.time - _timePressed >= DurationOrDefault)
					{
						context.PerformedAndStayPerformed();
						_currentHoldInterval = HoldInterval;
						context.SetTimeout(_currentHoldInterval);
					}

					if(!context.ControlIsActuated())
					{
						// Control is no longer actuated so we're done.
						context.Canceled();
					}

					break;

				case InputActionPhase.Performed:
					if(!context.ControlIsActuated(PressPointOrDefault))
					{
						context.Canceled();
					}

					break;
			}
		}

		public void Reset()
		{
			_timePressed = 0;
			_currentHoldInterval = 0;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// UI that is displayed when editing <see cref="HoldTriggerInteraction" /> in the editor.
	/// </summary>
	[UsedImplicitly]
	public class HoldInteractionEditor : InputParameterEditor<HoldTriggerInteraction>
	{
		protected override void OnEnable()
		{
			_pressPointSetting.Initialize("Press Point",
				"Float value that an axis control has to cross for it to be considered pressed.",
				"Default Button Press Point",
				() => target.PressPoint, v => target.PressPoint = v, () => InputSystem.settings.defaultButtonPressPoint);
			_durationSetting.Initialize("Hold Time",
				"Time (in seconds) that a control has to be held in order for it to register as a hold.",
				"Default Hold Time",
				() => target.Duration, x => target.Duration = x, () => InputSystem.settings.defaultHoldTime);
		}

		public override void OnGUI()
		{
		}

		public override void OnDrawVisualElements(VisualElement root, Action onChangedCallback)
		{
			_pressPointSetting.OnDrawVisualElements(root, onChangedCallback);
			_durationSetting.OnDrawVisualElements(root, onChangedCallback);

			FloatField holdIntervalField = new("Hold Interval") { value = target.HoldInterval };
			holdIntervalField.RegisterValueChangedCallback(evt =>
			{
				target.HoldInterval = evt.newValue;
				onChangedCallback();
			});
			holdIntervalField.tooltip = "Time (in seconds) between subsequent performed events while holding.";
			root.Add(holdIntervalField);

			FloatField holdIntervalMinField = new("Hold Interval Min") { value = target.HoldIntervalMin };
			holdIntervalMinField.RegisterValueChangedCallback(evt =>
			{
				target.HoldIntervalMin = evt.newValue;
				onChangedCallback();
			});
			holdIntervalMinField.tooltip = "Minimum time (in seconds) between subsequent performed events while holding.";
			root.Add(holdIntervalMinField);

			FloatField holdIntervalSpeedUpScaleField = new("Hold Interval Speed Up Scale") { value = target.HoldIntervalSpeedUpScale };
			holdIntervalSpeedUpScaleField.RegisterValueChangedCallback(evt =>
			{
				target.HoldIntervalSpeedUpScale = evt.newValue;
				onChangedCallback();
			});
			holdIntervalSpeedUpScaleField.tooltip = "Percentage (0-1) to multiply the hold interval with every time it triggers while holding.";
			root.Add(holdIntervalSpeedUpScaleField);
		}

		private readonly CustomOrDefaultSetting _pressPointSetting = new();
		private readonly CustomOrDefaultSetting _durationSetting = new();
	}

	/// <summary>
	/// Helper for parameters that have defaults (usually from <see cref="InputSettings" />).
	/// </summary>
	/// <remarks>
	/// Has a bool toggle to switch between default and custom value.
	/// </remarks>
	public class CustomOrDefaultSetting
	{
		private Func<float> _getValue;
		private Action<float> _setValue;
		private Func<float> _getDefaultValue;
		private bool _useDefaultValue;
		private bool _defaultComesFromInputSettings;
		private float _defaultInitializedValue;
		private GUIContent _toggleLabel;
		private GUIContent _valueLabel;
		private GUIContent _helpBoxText;
		private FloatField _floatField;
		private Button _openInputSettingsButton;
		private Toggle _defaultToggle;
		private HelpBox _helpBox;

		public void Initialize(string label, string tooltip, string defaultName, Func<float> getValue,
			Action<float> setValue, Func<float> getDefaultValue, bool defaultComesFromInputSettings = true,
			float defaultInitializedValue = 0)
		{
			_getValue = getValue;
			_setValue = setValue;
			_getDefaultValue = getDefaultValue;
			_toggleLabel = EditorGUIUtility.TrTextContent("Default",
				defaultComesFromInputSettings
					? $"If enabled, the default {label.ToLowerInvariant()} configured globally in the input settings is used. See Edit >> Project Settings... >> Input (NEW)."
					: "If enabled, the default value is used.");
			_valueLabel = EditorGUIUtility.TrTextContent(label, tooltip);

			_defaultInitializedValue = defaultInitializedValue;
			_useDefaultValue = Mathf.Approximately(getValue(), defaultInitializedValue);
			_defaultComesFromInputSettings = defaultComesFromInputSettings;
			_helpBoxText =
				EditorGUIUtility.TrTextContent(
					$"Uses \"{defaultName}\" set in project-wide input settings.");
		}

		public void OnDrawVisualElements(VisualElement root, Action onChangedCallback)
		{
			float value = _getValue();

			if(_useDefaultValue)
			{
				value = _getDefaultValue();
			}

			// If previous value was an epsilon away from default value, it most likely means that value was set by our own code down in this method.
			// Revert it back to default to show a nice readable value in UI.
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if(value - float.Epsilon == _defaultInitializedValue)
			{
				value = _defaultInitializedValue;
			}

			VisualElement container = new();
			container.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
			container.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

			VisualElement settingsContainer = new() { style = { flexDirection = FlexDirection.Row } };

			_floatField = new FloatField(_valueLabel.text) { value = value };
			_floatField.Q("unity-text-input").AddToClassList("float-field");
			_floatField.RegisterValueChangedCallback(ChangeSettingValue);
			_floatField.RegisterCallback<BlurEvent>(_ => OnEditEnd(onChangedCallback));
			_floatField.SetEnabled(!_useDefaultValue);

			_helpBox = new HelpBox(_helpBoxText.text, HelpBoxMessageType.None);

			_defaultToggle = new Toggle("Default")
			{
				value = _useDefaultValue,
				style =
				{
					flexDirection = FlexDirection.RowReverse
				}
			};
			_defaultToggle.RegisterValueChangedCallback(evt => ToggleUseDefaultValue(evt, onChangedCallback));
			_defaultToggle.Q<Label>().style.minWidth = new StyleLength(StyleKeyword.Auto);

			VisualElement buttonContainer = new()
			{
				style =
				{
					flexDirection = FlexDirection.RowReverse
				}
			};

			settingsContainer.Add(_floatField);
			settingsContainer.Add(_defaultToggle);
			container.Add(settingsContainer);

			if(_useDefaultValue)
			{
				container.Add(_helpBox);
			}

			container.Add(buttonContainer);

			root.Add(container);
		}

		private void OnAttachToPanel(AttachToPanelEvent evt)
		{
			// Monitor changes to settings for as long as the panel is attached to a visual tree
			InputSystem.onSettingsChange += InputSystemOnSettingsChange;
		}

		private void OnDetachFromPanel(DetachFromPanelEvent evt)
		{
			// Stop monitoring changes to settings when panel is no longer part of a visual tree
			InputSystem.onSettingsChange -= InputSystemOnSettingsChange;
		}

		private void InputSystemOnSettingsChange()
		{
			// Default value may change at any point settings are modified so fetch current default value
			// if currently configured to display default value and having default coming from settings.
			if(_floatField != null && _useDefaultValue && _defaultComesFromInputSettings)
			{
				_floatField.value = _getDefaultValue();
			}
		}

		private void ChangeSettingValue(ChangeEvent<float> evt)
		{
			if(!_useDefaultValue)
			{
				SetValue(evt.newValue);
			}
		}

		private static void OnEditEnd(Action onChangedCallback)
		{
			onChangedCallback.Invoke();
		}

		private void ToggleUseDefaultValue(ChangeEvent<bool> evt, Action onChangedCallback)
		{
			if(evt.newValue != _useDefaultValue)
			{
				_setValue(!evt.newValue ? _getDefaultValue() : _defaultInitializedValue);
				onChangedCallback.Invoke();
			}

			_useDefaultValue = evt.newValue;
			_floatField?.SetEnabled(!_useDefaultValue);
		}

		private void SetValue(float newValue)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if(newValue == _defaultInitializedValue)
			{
				// If user sets a value that is equal to default initialized, change value slightly so it doesn't pass potential default checks.
				_setValue(newValue + float.Epsilon);
			}
			else
			{
				_setValue(newValue);
			}
		}

		public void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(_useDefaultValue);

			float value = _getValue();

			if(_useDefaultValue)
			{
				value = _getDefaultValue();
			}

			// If previous value was an epsilon away from default value, it most likely means that value was set by our own code down in this method.
			// Revert it back to default to show a nice readable value in UI.
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if(value - float.Epsilon == _defaultInitializedValue)
			{
				value = _defaultInitializedValue;
			}

			float newValue = EditorGUILayout.FloatField(_valueLabel, value, GUILayout.ExpandWidth(false));
			if(!_useDefaultValue)
			{
				SetValue(newValue);
			}

			EditorGUI.EndDisabledGroup();

			bool newUseDefault = GUILayout.Toggle(_useDefaultValue, _toggleLabel, GUILayout.ExpandWidth(false));
			if(newUseDefault != _useDefaultValue)
			{
				if(!newUseDefault)
				{
					_setValue(_getDefaultValue());
				}
				else
				{
					_setValue(_defaultInitializedValue);
				}
			}

			_useDefaultValue = newUseDefault;
			EditorGUILayout.EndHorizontal();

			// If we're using a default from global InputSettings, show info text for that and provide
			// button to open input settings.
			if(_useDefaultValue && _defaultComesFromInputSettings)
			{
				EditorGUILayout.HelpBox(_helpBoxText);
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
		}
	}
#endif
}