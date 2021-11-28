namespace GitDb
{
    public class StaticUserProvider : IUserProvider
    {
        private readonly string _name;

        public StaticUserProvider(string name)
        {
            _name = name;
        }

        public string Username => _name;
        public string Email => _name;
    }
}