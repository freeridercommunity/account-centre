using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

public partial class FormDialog : ContentDialog
{
	// public event EventHandler<FormSubmitEventArgs>? Submit;
	protected UIElementCollection Children
	{
		get => this.StackPanel.Children;
		set
		{
			this.StackPanel.Children.Clear();
			foreach (var child in value)
				this.StackPanel.Children.Add(child);
		}
	}
	protected readonly InfoBar ErrorBar = new()
	{
		Severity = InfoBarSeverity.Error,
		IsOpen = true
	};
	protected bool IsLoadingVisible => this.Content is ProgressRing;
	protected double Spacing
	{
		get => this.StackPanel.Spacing;
		set => this.StackPanel.Spacing = value;
	}
	private readonly StackPanel StackPanel = new()
	{
		Spacing = 8
	};
	public FormDialog(UIElement[]? elements = null)
	{
		if (elements != null)
			foreach (var element in elements)
				this.StackPanel.Children.Add(element);

		this.CloseButtonText = "Cancel";
		this.Content = StackPanel;
		this.DefaultButton = ContentDialogButton.Primary;
		this.PrimaryButtonText = "Submit";
	}

	protected virtual void ToggleLoading(bool? force = null)
		=> this.Content = (force != null ? (bool)force : this.IsLoadingVisible)
			? new ProgressRing
			{
				Height = 32,
				Width = 32
			}
			: this.StackPanel;

	protected void ShowError(string message)
	{
		this.ErrorBar.Message = message;
		if (!this.StackPanel.Children.Contains(ErrorBar))
			this.StackPanel.Children.Add(ErrorBar);

		this.ErrorBar.IsOpen = true;
	}

	protected void ClearError()
	{
		this.StackPanel.Children.Remove(this.ErrorBar);
		this.ErrorBar.IsOpen = false;
	}

	// public class FormSubmitEventArgs : CancelEventArgs
	// {
	// 	public object? Data { get; set; }
	// 	public FormSubmitEventArgs(object? data = null)
	// 	{
	// 		this.Data = data;
	// 	}
	// }
}