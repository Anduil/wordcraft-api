using System.ComponentModel.DataAnnotations;
using Models.Entities;

namespace Models
{
    public class User : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }

        public override string ToString()
        {
            return $"{Id} {Email} {PasswordHash} {PasswordSalt}";
        }
    }
}
