using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffManagement.Application.Models
{
    public class CreateStaffDto
    {
        public string StaffId { get; set; } = null!; 
        public string FullName { get; set; } = null!;
        public DateOnly? BirthDay { get; set; }
        public int? Gender { get; set; }
    }
}
