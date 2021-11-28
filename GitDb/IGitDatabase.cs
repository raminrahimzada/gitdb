using System.Collections.Generic;

namespace GitDb
{
    public interface IGitDatabase
    {
        string Insert<T>(string id, T data);
        string Update<T>(string id, T data);
        string Delete<T>(string id);
        string Revert(string sha);
        IEnumerable<ChangeHistory<T>> GetChanges<T>(string id);
        T Get<T>(string id);
        void DestroyDatabase();
    }
}