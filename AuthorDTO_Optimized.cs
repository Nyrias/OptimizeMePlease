using System.Collections.Generic;

namespace OptimizeMePlease
{
    public sealed class AuthorDTO_Optimized
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public IEnumerable<BookDTO_Optimized> Books { get; set; }
    }
}