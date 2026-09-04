using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Models.DTOs
{
    public class WorkItemDateRangeSummaryDto
    {
        public bool IsBillable { get; set; } = true;

        public bool IsInReview { get; set; } = false;

        public bool IsOpen { get; set; } = true;

        public int TotalMinutesWorked { get; set; } = 0;
     
        public int WorkItemID { get; set; } = 0;

        public string WorkItemTitle { get; set; } = string.Empty;
    }
}
