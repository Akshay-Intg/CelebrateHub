using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class MyTodayDto
    {
        public bool IsBirthday { get; set; }
        public bool IsAnniversary { get; set; }
        public int TurningAge { get; set; }
        public int YearsOfService { get; set; }
        public string Department { get; set; } = string.Empty;
    }
}
