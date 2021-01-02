namespace Klavogonki
{
    public class User
    {
        public int Id { get; private set; }
        public string Nick { get; private set; }

        public User(int id, string nick)
        {
            Id = id;
            Nick = nick;
        }
    }
}
