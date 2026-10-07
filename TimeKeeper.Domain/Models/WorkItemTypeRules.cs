namespace TimeKeeper.Domain.Models;

public static class WorkItemTypeRules
{
    public static void ValidateAssignment(int? selectedId, int? previousId, int? existingTypeId, bool enabled)
    {
        if (selectedId == null) throw new InvalidOperationException("Work Item Type is required.");
        if (existingTypeId != selectedId || (!enabled && previousId != selectedId))
            throw new InvalidOperationException("Select an enabled Activity Type. The selected type may have changed; reopen the form to refresh choices.");
    }
}
