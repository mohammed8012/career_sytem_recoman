using System;

namespace career_sytem_recoman.Models.Entities
{
    public partial class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }

        public virtual User User { get; set; } = null!;
    }
}