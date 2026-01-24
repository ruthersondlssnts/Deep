using Deep.Common.Domain;

namespace Deep.Programs.Domain.Users
{
    internal sealed class User
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public Role Role { get; private set; }

        private User() { }

        public static User Create(
            Guid id,
            string firstName,
            string lastName,
            string email,
            Role role)
        {
            return new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role
            };
        }
    }

}
