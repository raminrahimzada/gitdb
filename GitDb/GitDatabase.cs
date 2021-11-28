using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitDb
{
    public class GitDatabase: IGitDatabase
    {
        public DatabaseOptions Options { get; }
        private Repository _repo;
        private Signature _signature;

        public GitDatabase(DatabaseOptions options)
        {
            Options = options;
            Init();
        }

        private void Init()
        {
            _signature = new Signature(Options.UserProvider.Username, Options.UserProvider.Email,
                DateTimeOffset.UtcNow);
            if (!Repository.IsValid(Options.Location))
            {
                Repository.Init(Options.Location);
            }

            _repo = new Repository(Options.Location);
        }

        public GitDatabase(string location) 
        {
            Options = DatabaseOptions.Default(location);
            Init();
        }

        private string GetFullPath<T>(string id)
        {
            var folder = typeof(T).Name;
            folder = Path.Combine(Options.Location, folder);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, Helper.GetFileName(id));
        }

        public string Insert<T>(string id, T data)
        {
            Helper.ThrowIfInvalidFileName(ref id);
            var fullPath = GetFullPath<T>(id);
            if (!File.Exists(fullPath))
            {
                var buffer = Options.Serializer.Serialize(ref data);
                File.WriteAllBytes(fullPath, buffer);
                Commands.Stage(_repo, "*");
                var message = Options.CommitMessageProvider.GenerateInsertMessage(id, data);
                var commit = _repo.Commit(message, _signature, _signature);
                return commit.Id.ToString();
            }

            throw new Exception("Duplicate Entity with id=" + id);
        }

        public string Update<T>(string id, T data)
        {
            Helper.ThrowIfInvalidFileName(ref id);
            var fullPath = GetFullPath<T>(id);
            if (File.Exists(fullPath))
            {
                var buffer = Options.Serializer.Serialize(ref data);
                File.WriteAllBytes(fullPath, buffer);
                Commands.Stage(_repo, "*");
                var message = Options.CommitMessageProvider.GenerateUpdateMessage(id, data);
                var commit = _repo.Commit(message, _signature, _signature);
                return commit.Id.ToString();
            }
            throw new Exception("Object not found id=" + id);
        }

        public string Delete<T>(string id)
        {
            Helper.ThrowIfInvalidFileName(ref id);
            var fullPath = GetFullPath<T>(id);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Commands.Stage(_repo, "*");
                var message = Options.CommitMessageProvider.GenerateDeleteMessage<T>(id);
                var commit = _repo.Commit(message, _signature, _signature);
                return commit.Id.ToString();
            }
            throw new Exception("Object not found id=" + id);
        }

        public T Get<T>(string id)
        {
            Helper.ThrowIfInvalidFileName(ref id);
            var fullPath = GetFullPath<T>(id);
            if (File.Exists(fullPath))
            {
                var buffer = File.ReadAllBytes(fullPath);
                return Options.Serializer.Deserialize<T>(ref buffer);
            }

            return default;
        }

        public IEnumerable<ChangeHistory<T>> GetChanges<T>(string id)
        {
            Helper.ThrowIfInvalidFileName(ref id);
            var path = Helper.GetPath<T>(id);

            //TODO use this method instead of full commit enumerating
            //TODO _repo.Commits.QueryBy(path)
            //TODO when this bug solved:
            //TODO https://github.com/libgit2/libgit2sharp/issues/1401

            var commits = _repo.Commits.QueryBy(new CommitFilter
            {
                SortBy = default,
                FirstParentOnly = true,
            });

            foreach (var commit in commits)
            {
                var history = new ChangeHistory<T>()
                    .SetMessage(commit.MessageShort)
                    .SetUser(commit.Author.Name)
                    .SetWhen(commit.Author.When);

                var parentCommit = commit.Parents.SingleOrDefault();
                if (parentCommit != null)
                {
                    var changes = _repo.Diff.Compare<TreeChanges>(parentCommit.Tree, commit.Tree);
                    var deleted = changes.Deleted.ToArray();
                    var added = changes.Added.ToArray();
                    var modified = changes.Modified.ToArray();
                    foreach (var e in deleted)
                    {
                        if (!Helper.IsSamePath(e.Path, path)) continue;
                        if (e.Status != ChangeKind.Deleted) continue;

                        yield return history.Deleted();
                    }

                    foreach (var e in added)
                    {
                        if (!Helper.IsSamePath(e.Path, path)) continue;
                        if (e.Status != ChangeKind.Added) continue;
                        var entryList = commit.GetAllBlobs();
                        foreach (var entry in entryList)
                        {
                            Debug.Assert(entry.TargetType == TreeEntryTargetType.Blob);
                            var blob = (Blob)entry.Target;
                            using (var stream = blob.GetContentStream())
                            {
                                var t = Options.Serializer.Deserialize<T>(stream);
                                yield return history.Inserted(t);
                            }
                        }
                    }
                    
                    foreach (var e in modified)
                    {
                        if (!Helper.IsSamePath(e.Path, path)) continue;
                        if (e.Status != ChangeKind.Modified) continue;
                        var entryList = commit.GetAllBlobs();
                        foreach (var entry in entryList)
                        {
                            Debug.Assert(entry.TargetType == TreeEntryTargetType.Blob);
                            var blob = (Blob)entry.Target;
                            using (var stream = blob.GetContentStream())
                            {
                                var t = Options.Serializer.Deserialize<T>(stream);
                                yield return history.Updated(t);
                            }
                        }
                    }
                }
                else
                {
                    var treeList = commit.GetAllBlobs();
                    foreach (var entry in treeList)
                    {
                        if (!Helper.IsSamePath(entry.Path, path)) continue;
                        Debug.Assert(entry.TargetType == TreeEntryTargetType.Blob);
                        var blob = (Blob)entry.Target;
                        using (var stream = blob.GetContentStream())
                        {
                            var t = Options.Serializer.Deserialize<T>(stream);
                            yield return history.Inserted(t);
                        }
                    }
                }
            }
        }

        public string Revert(string sha)
        {
            Helper.ThrowIfInvalidFileName(ref sha);
            var commit = _repo.Lookup<Commit>(sha);
            var options=new RevertOptions()
            {
                FileConflictStrategy = CheckoutFileConflictStrategy.Merge
            };
            var result = _repo.Revert(commit, _signature, options);
            if (result.Status == RevertStatus.Reverted)
            {
                return result.Commit.Id.ToString();
            }

            throw new Exception(result.Status.ToString());
        }

        public void DestroyDatabase()
        {
            var files = Directory.EnumerateFiles(Options.Location, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            var dirs = Directory.EnumerateDirectories(Options.Location, "*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            Directory.Delete(Options.Location, true);
        }
    }
}