using System.Diagnostics.Eventing.Reader;

namespace DevHabit.Api.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);
