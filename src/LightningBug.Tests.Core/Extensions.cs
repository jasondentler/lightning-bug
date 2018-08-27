using System.Collections.Generic;

namespace LightningBug
{
    public static class Extensions
    {

        public static IEnumerable<int> To(this int start, int end)
        {
            for (var i = start; i <= end; i++)
                yield return i;
        }

    }
}
