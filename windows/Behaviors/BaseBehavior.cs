using Microsoft.UI.Xaml;

namespace AccountCentre.Behaviors;

public interface IBehavior<TTarget>
{
	static virtual void OnAttached(TTarget target) { }
	static virtual void OnDetached(TTarget target) { }
}

public abstract class BaseBehavior<TSelf, TTarget> where TSelf : IBehavior<TTarget>
{
	public static readonly DependencyProperty EnabledProperty =
		DependencyProperty.RegisterAttached(
			"Enabled",
			typeof(bool),
			typeof(TSelf),
			new PropertyMetadata(false, OnEnabledChanged));
	public static bool GetEnabled(DependencyObject obj)
		=> (bool)obj.GetValue(EnabledProperty);
	public static void SetEnabled(DependencyObject obj, bool value)
		=> obj.SetValue(EnabledProperty, value);

	private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not TTarget target)
			return;

		if ((bool)e.NewValue)
			TSelf.OnAttached(target);
		else
			TSelf.OnDetached(target);
	}
}