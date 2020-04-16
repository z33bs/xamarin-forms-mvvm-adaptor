namespace XamarinFormsMvvmAdaptor
{
	/// <summary>
    /// Extension methods for the <see cref="string"/> object
    /// </summary>
    public static class SystemStringExtensions
    {
		/// <summary>
        /// Replaces the last occuring substring defined by <paramref name="find"/>
        /// with given string <paramref name="replace"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
		public static string ReplaceLastOccurrence(this string source, string find, string replace)
		{
			int place = source.LastIndexOf(find);

			if (place == -1)
				return source;

			return source.Remove(place, find.Length).Insert(place, replace);
		}
	}
}
