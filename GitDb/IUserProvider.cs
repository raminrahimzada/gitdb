namespace GitDb
{
    public interface IUserProvider
    {
        public string Username { get;}
        public string Email { get;  }
    }
}