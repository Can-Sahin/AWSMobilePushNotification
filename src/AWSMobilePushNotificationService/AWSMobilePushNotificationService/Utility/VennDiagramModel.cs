using System;
using System.Collections.Generic;
using System.Linq;

namespace AWSMobilePushNotificationService.Utility
{
    internal class VennDiagramModel
    {
        public static Tuple<IEnumerable<TFirst>, IEnumerable<TFirst>, IEnumerable<TSecond>> Create<TFirst, TSecond, TCompared>(
                 IEnumerable<TFirst> first,
                 IEnumerable<TSecond> second,
                 Func<TFirst, TCompared> firstSelect,
                 Func<TSecond, TCompared> secondSelect)
        {
            return Create(first, second, firstSelect, secondSelect, EqualityComparer<TCompared>.Default);
        }

        public static Tuple<IEnumerable<TFirst>, IEnumerable<TFirst>, IEnumerable<TSecond>> Create<TFirst, TSecond, TCompared>(
            IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TCompared> firstSelect,
            Func<TSecond, TCompared> secondSelect,
            IEqualityComparer<TCompared> comparer)
        {
            if (first == null)
            {
                if (second == null)
                {
                    return null;
                }
                return new Tuple<IEnumerable<TFirst>, IEnumerable<TFirst>, IEnumerable<TSecond>>(null, null, second);
            }
            else if (second == null)
            {
                return new Tuple<IEnumerable<TFirst>, IEnumerable<TFirst>, IEnumerable<TSecond>>(first, new List<TFirst>(), new List<TSecond>());
            }
            var leftDifference = ExceptIterator<TFirst, TSecond, TCompared>(first, second, firstSelect, secondSelect, comparer);
            var rightDifference = ExceptIterator<TSecond, TFirst, TCompared>(second, first, secondSelect, firstSelect, comparer);
            var intersection = first.Where(f => second.Any(s => secondSelect(s).Equals(firstSelect(f))));
            return new Tuple<IEnumerable<TFirst>, IEnumerable<TFirst>, IEnumerable<TSecond>>(leftDifference, intersection, rightDifference);
        }

        private static IEnumerable<TFirst> ExceptIterator<TFirst, TSecond, TCompared>(
            IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TCompared> firstSelect,
            Func<TSecond, TCompared> secondSelect,
            IEqualityComparer<TCompared> comparer)
        {
            HashSet<TCompared> set = new HashSet<TCompared>(second.Select(secondSelect), comparer);
            foreach (TFirst tSource1 in first)
            {
                if (!set.Contains(firstSelect(tSource1)))
                {
                    yield return tSource1;
                }
            }
        }
    }
}
