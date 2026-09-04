namespace TimeKeeper.App.Api.Enums;


public enum TimeEntrySaveActions
{
    None            = -1,
    StartWork       = 0,
    Update          = 1,
    ContinueLater   = 2,  
    PRSubmitted     = 3,
    Complete        = 4,
    InsertTimeEntry = 5,
}
