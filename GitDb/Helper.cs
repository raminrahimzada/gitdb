using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitDb
{
    internal static class Helper
    {
        private static IEnumerable<TreeEntry> _(TreeEntry e)
        {
            if (e.TargetType == TreeEntryTargetType.Tree)
            {
                var t = (Tree)e.Target;
                foreach (var entry in t)
                {
                    switch (entry.TargetType)
                    {
                        case TreeEntryTargetType.Blob:
                            yield return entry;
                            break;

                        case TreeEntryTargetType.Tree:
                            var inner = _(entry);
                            foreach (var treeEntry in inner)
                            {
                                yield return treeEntry;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (e.TargetType == TreeEntryTargetType.Blob)
            {
                yield return e;
            }
        }
        internal static IEnumerable<TreeEntry> GetAllBlobs(this Commit commit)
        {
            foreach (var entry in commit.Tree)
            {
                if (entry.TargetType == TreeEntryTargetType.Blob) yield return entry;
                else if(entry.TargetType==TreeEntryTargetType.Tree)
                {
                    foreach (var treeEntry in _(entry))
                    {
                        yield return treeEntry;
                    }
                }
            }
        }

        internal static bool IsSamePath(string path1, string path2)
        {
            char[] separator = { '/', '\\' };
            var arr1 = path1.Split(separator);
            var arr2 = path2.Split(separator);
            return arr1.SequenceEqual(arr2);
        }

        internal static string GetFileName(string id)
        {
            return id + ".json";
        }

        internal static string GetPath<T>(string id)
        {
            var folder = typeof(T).Name;
            return Path.Combine(folder, GetFileName(id));
        }

        public static void ThrowIfInvalidFileName(ref string id)
        {
            const string charsetExtra = "-.";
            if (string.IsNullOrWhiteSpace(id)) goto boom;

            var hasInvalidChar = id.Where(ch => !char.IsLetterOrDigit(ch)).Any(ch => !charsetExtra.Contains((char)ch));
            if (hasInvalidChar) goto boom;

            return;

            boom:
            throw new Exception("Invalid id " + id);
        }
    }
}