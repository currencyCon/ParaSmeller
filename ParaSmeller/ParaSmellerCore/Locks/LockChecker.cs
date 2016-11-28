using System.Collections.Generic;

namespace ParaSmellerCore.Locks
{
    public static class LockChecker
    {
        public static bool IsCorrectlyAquired(List<List<string>> lockObjects)
        {
            foreach (var l1 in lockObjects)
            {
                foreach (var l2 in lockObjects)
                {
                    if (l1 == l2) continue;

                    if (!IsAquiredCorrectOnLists(l1, l2))
                        return false;
                }
            }

            return true;
        }

        private static bool IsAquiredCorrectOnLists(List<string> l1, List<string> l2)
        {
            if (l1.Count < 2 || l2.Count < 2) return true;

            if (!IsAquiredSequenceCorrect(l1, l2)) return false;
            if (!IsAquiredSequenceCorrect(l2, l1)) return false;

            return true;
        }

        private static bool IsAquiredSequenceCorrect(IReadOnlyList<string> baseList, List<string> secondList)
        {
            for (var i = 0; i < baseList.Count; i++)
            {
                for (var j = i; j < baseList.Count; j++)
                {
                    if (i == j) continue;

                    var item1List1 = baseList[i];
                    var item2List1 = baseList[j];

                    var index1List2 = secondList.FindIndex(s => s == item1List1);
                    var index2List2 = secondList.FindIndex(s => s == item2List1);

                    if (index1List2 == -1 || index2List2 == -1) continue;
                    if (index1List2 > index2List2) return false;
                }
            }

            return true;
        }
    }
}
