using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Enums;

public enum ActivityTypeCategories
{
    Unset = 0,

    /// <summary>
    ///  Indicates that the associated Activity/WorkItem is a Work related administrative event.
    ///  (e.g. Update PIV card)
    /// </summary>
    BusinessAdmin = 1,

    /// <summary>
    ///  Indicates that the associated Activity/WorkItem is a Coding activity (Bugfix, Feature, 
    ///  Development, TechDebt, etc.)
    /// </summary>
    Code = 2,

    /// <summary>
    ///  Indicates that the associated Activity/WorkItem is a Meeting activity (Daily Stand Up, 
    ///  Weekly Sync, etc.)
    /// </summary>
    Meeting = 3,

    /// <summary>
    ///  Indicates that the associated Activity/WorkItem represents time away from work. (e.g. PTO, 
    ///  lunch, Personal Appointment). 
    /// </summary>
    TimeAway = 4,

    /// <summary>
    ///  Indicates that the associated Activity/WorkItem is a Training event.
    /// </summary>
    Training = 5
}
