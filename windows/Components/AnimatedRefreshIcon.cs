using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace AccountCentre.Components;

public sealed partial class AnimatedRefreshIcon : FontIcon
{
	private const double DefaultFrom = 0;
	private const double DefaultTo = 90;

	private static readonly Duration DefaultDuration = new(TimeSpan.FromMilliseconds(200));
	private static readonly CubicEase DefaultEasingFunction = new()
	{
		EasingMode = EasingMode.EaseInOut
	};
	private static readonly RepeatBehavior DefaultRepeatBehavior = new(1);
	private static readonly Duration SpinningDuration = new(TimeSpan.FromMilliseconds(800));

	private readonly DoubleAnimation _animation = new();
	private readonly RotateTransform _rotation = new()
	{
		Angle = 0
	};
	private readonly Storyboard _storyboard = new();
	public AnimatedRefreshIcon()
	{
		_pressedHandler = HandlePressed;
		_enteredHandler = HandleEntered;
		_exitedHandler = HandleExited;
		_releasedHandler = HandleReleased;

		Reset();

		FontSize = 16;
		Glyph = "\uE72C";
		RenderTransform = _rotation;
		RenderTransformOrigin = new Point(0.5, 0.5);

		Storyboard.SetTarget(_animation, _rotation);
		Storyboard.SetTargetProperty(_animation, "Angle");

		_storyboard.Children.Add(_animation);

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	public void StartSpinning()
	{
		_storyboard.Pause();

		_animation.From = _rotation.Angle;
		_animation.To = _rotation.Angle + 360;
		_animation.Duration = SpinningDuration;
		_animation.EasingFunction = null;
		_animation.RepeatBehavior = RepeatBehavior.Forever;

		_storyboard.Begin();
	}

	public void StopSpinningGracefully()
	{
		_storyboard.Pause();

		var angle = _rotation.Angle;

		if (angle == 0)
		{
			_storyboard.Stop();
			return;
		}

		_animation.From = angle;
		_animation.To = 360;
		_animation.Duration = new Duration(
			TimeSpan.FromMilliseconds(
				SpinningDuration.TimeSpan.TotalMilliseconds *
				((360 - angle) / 360)));

		_animation.RepeatBehavior = DefaultRepeatBehavior;
		_animation.EasingFunction = null;

		_storyboard.Begin();
	}

	public void ForceStop()
	{
		_storyboard.Stop();
		_rotation.Angle = 0;
	}

	private void Reset()
	{
		_animation.Duration = DefaultDuration;
		_animation.EasingFunction = DefaultEasingFunction;
		_animation.From = DefaultFrom;
		_animation.RepeatBehavior = DefaultRepeatBehavior;
		_animation.To = DefaultTo;
	}

	private bool _buttonPressed;
	private readonly PointerEventHandler _pressedHandler;
	private void HandlePressed(object sender, PointerRoutedEventArgs e)
	{
		_buttonPressed = true;

		Reset();
		_storyboard.Begin();
	}

	private readonly PointerEventHandler _enteredHandler;
	private void HandleEntered(object sender, PointerRoutedEventArgs e)
	{
		if (!_buttonPressed)
			return;

		Reset();
		_storyboard.Begin();
	}

	private readonly PointerEventHandler _exitedHandler;
	private void HandleExited(object sender, PointerRoutedEventArgs e)
	{
		if (!_buttonPressed)
			return;

		_storyboard.Pause();

		_animation.From = _rotation.Angle;
		_animation.To = 0;

		_storyboard.Begin();
	}

	private readonly PointerEventHandler _releasedHandler;
	private void HandleReleased(object sender, PointerRoutedEventArgs e)
	{
		if (!_buttonPressed)
			return;

		_buttonPressed = false;

		if (sender is not Button button || !button.IsPointerOver)
			return;

		StartSpinning();
	}

	private Button? _parentButton;
	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (Parent is not Button button)
			return;

		_parentButton = button;

		button.AddHandler(
			UIElement.PointerPressedEvent,
			_pressedHandler,
			handledEventsToo: true);

		button.AddHandler(
			UIElement.PointerEnteredEvent,
			_enteredHandler,
			handledEventsToo: true);

		button.AddHandler(
			UIElement.PointerExitedEvent,
			_exitedHandler,
			handledEventsToo: true);

		button.AddHandler(
			UIElement.PointerReleasedEvent,
			_releasedHandler,
			handledEventsToo: true);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (_parentButton is null)
			return;

		_parentButton.RemoveHandler(
			UIElement.PointerPressedEvent,
			_pressedHandler);

		_parentButton.RemoveHandler(
			UIElement.PointerEnteredEvent,
			_enteredHandler);

		_parentButton.RemoveHandler(
			UIElement.PointerExitedEvent,
			_exitedHandler);

		_parentButton.RemoveHandler(
			UIElement.PointerReleasedEvent,
			_releasedHandler);

		_parentButton = null;
	}
}