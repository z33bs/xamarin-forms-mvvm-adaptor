namespace XamarinFormsMvvmAdaptor
{
    public static class SystemStringExtensions
    {
		public static string ReplaceLastOccurrence(this string source, string find, string replace)
		{
			int place = source.LastIndexOf(find);

			if (place == -1)
				return source;

			return source.Remove(place, find.Length).Insert(place, replace);
		}
	}
}
