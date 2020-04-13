using System;
namespace XamarinFormsMvvmAdaptor
{
    public static class Helpers
    {
        //todo make as stringextension
		public static string ReplaceLastOccurrence(string source, string find, string replace)
		{
			int place = source.LastIndexOf(find);

			if (place == -1)
				return source;

			return source.Remove(place, find.Length).Insert(place, replace);
		}
	}
}
