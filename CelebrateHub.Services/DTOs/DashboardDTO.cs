using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class DashboardDto
    {
        public IEnumerable<BirthdayDto> TodaysBirthdays { get; set; } = [];
        public IEnumerable<BirthdayDto> UpcomingBirthdays { get; set; } = [];
        public IEnumerable<AnniversaryDto> TodaysAnniversaries { get; set; } = [];
        public IEnumerable<AnniversaryDto> UpcomingAnniversaries { get; set; } = [];
        public int TotalEmployees { get; set; }
    }
}
