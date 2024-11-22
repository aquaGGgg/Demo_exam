/*using Test_Demo_Ex.Models;

namespace Test_Demo_Ex.Repository
{
    public class UserRepository
    {
        private readonly List<User> _users = new();

        public User? GetUserByUsername(string username) =>
            _users.FirstOrDefault(u => u.Username == username);

        public void AddUser(User user) => _users.Add(user);
    }
}
*/