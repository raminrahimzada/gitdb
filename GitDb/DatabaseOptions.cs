namespace GitDb
{
    public class DatabaseOptions
    {
        public IObjectSerializer Serializer;
        public IUserProvider UserProvider;
        public ICommitMessageProvider CommitMessageProvider;
        public string Location;

        public DatabaseOptions(string location,IObjectSerializer serializer, IUserProvider userProvider, ICommitMessageProvider commitMessageProvider)
        {
            Location = location;
            Serializer = serializer;
            UserProvider = userProvider;
            CommitMessageProvider = commitMessageProvider;
        }

        public static DatabaseOptions Default(string location)
        {
            return new(location, new JsonObjectSerializer(), new StaticUserProvider("admin"),
                new DefaultCommitMessageProvider());
        }
    }
}