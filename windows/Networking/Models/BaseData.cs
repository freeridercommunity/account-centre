using System.Reflection;
using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public abstract record BaseData<T>
{
	// public object? this[string field]
	// {
	// 	get
	// 	{
	// 		var property = GetType().GetProperty(field);
	// 		return property?.GetValue(this);
	// 	}
	// 	set
	// 	{
	// 		var property = GetType().GetProperty(field);
	// 		property?.SetValue(this, value);
	// 	}
	// }

	public object? this[string field]
	{
		get
		{
			var property = GetType()
				.GetProperties()
				.FirstOrDefault(property =>
					property.Name.Equals(field, StringComparison.OrdinalIgnoreCase) ||
					property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
						.Equals(field, StringComparison.OrdinalIgnoreCase) == true);

			return property?.GetValue(this);
		}

		set
		{
			var property = GetType()
				.GetProperties()
				.FirstOrDefault(property =>
					property.Name.Equals(field, StringComparison.OrdinalIgnoreCase) ||
					property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
						.Equals(field, StringComparison.OrdinalIgnoreCase) == true);

			property?.SetValue(this, value);
		}
	}

	public static string? ResolveJsonPropertyName(string field)
	{
		var property = typeof(T)
			.GetProperties()
			.FirstOrDefault(property =>
				property.Name.Equals(field, StringComparison.OrdinalIgnoreCase) ||
				property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
					.Equals(field, StringComparison.OrdinalIgnoreCase) == true);

		return property?
			.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
			?? property?.Name;
	}
}