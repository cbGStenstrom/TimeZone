using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Enums;

public enum BooleanOptions
{
    All,
    False,
    True,
}
public enum IsBillableOptions
{
    All,
    Billable,
    NonBillable,
}

public enum IsOpenOptions
{
    All,
    Closed,
    InReview,
    Open,
}

public enum WorkItemType
{
    Bugfix,
    Chore,
    Feature,
}