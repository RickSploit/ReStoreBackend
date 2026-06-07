using System.Collections.Generic;

namespace ReStore.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public string PhoneNumber { get; set; } = string.Empty;
    }
}


