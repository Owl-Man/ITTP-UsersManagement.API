namespace ITTP.UsersManagement.API.Core.Models
{
    public class User
    {
        public Guid Id { get; }
        public string Login { get; }
        public string Password { get; }
        
        public string Name { get; }
        public int Gender { get; }
        public DateTime? Birthday { get; }
        
        public bool Admin { get; }
        public DateTime CreatedOn { get; }
        public string CreatedBy { get; }
        public DateTime ModifiedOn { get; }
        public string ModifiedBy { get; }
        public DateTime RevokedOn { get; }
        public string RevokedBy { get; }
    }
}