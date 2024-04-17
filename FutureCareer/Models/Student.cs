using System.ComponentModel.DataAnnotations;

namespace FutureCareer.Models
{
    public class Student
    {
        public Guid StudentID { get; set; }


        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(25, MinimumLength = 4, ErrorMessage = "The Name must be between 4 and 25 characters.")]
        [Display(Name = "Name")]
        public string Name { get; set; }


        [Required(ErrorMessage = "The Surname field is required.")]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "The Surname must be between 4 and 8 characters.")]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Contact field is required.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Contact")]
        public string Contact { get; set; }

        [Display(Name = "Enrollment Status")]
        public bool EnrollmentStatus { get; set; }
    }
}
