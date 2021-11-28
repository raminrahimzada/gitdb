namespace GitDb
{
    public class DefaultCommitMessageProvider: ICommitMessageProvider
    {
        public string GenerateInsertMessage<T>(string id, T entity)
        {
            return $"insert into {(typeof(T).Name)} id={id}";
        }

        public string GenerateUpdateMessage<T>(string id, T entity)
        {
            return $"update {(typeof(T).Name)} id={id}";
        }

        public string GenerateDeleteMessage<T>(string id)
        {
            return $"delete {(typeof(T).Name)} id={id}";
        }
    }
}