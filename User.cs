namespace Projekt_studia2
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public User()
        {
        }

        public User(int id, string username, string password, string role)
        {
            ID = id;
            Username = username;
            Password = password;  
            Role = role;
        }
    }
}
