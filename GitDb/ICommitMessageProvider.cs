namespace GitDb
{
    public interface ICommitMessageProvider
    {
        string GenerateInsertMessage<T>(string id, T entity);
        string GenerateUpdateMessage<T>(string id, T entity);
        string GenerateDeleteMessage<T>(string id);
    }
}