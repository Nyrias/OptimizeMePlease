using System.Collections.Generic;

namespace OptimizeMePlease
{
    public struct AuthorDTO_OptimizedStruct
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public IEnumerable<BookDTO_OptimizedStruct> Books { get; set; }
    }
}