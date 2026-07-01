namespace GymManagement.Data.Queries
{
    public static class NavbarSearchQueries
    {
        public const string SearchMembers = @"
            SELECT TOP 5 MemberId, MemberName, MemberCode, MobileNo
            FROM MemberMasters
            WHERE MemberName LIKE @Q
               OR MemberCode LIKE @Q
               OR MobileNo LIKE @Q
            ORDER BY MemberId DESC";

        public const string SearchClasses = @"
            SELECT TOP 5 ClassId, ClassName, Schedule
            FROM ClassMasters
            WHERE ClassName LIKE @Q
               OR Schedule LIKE @Q
            ORDER BY ClassId DESC";

        public const string SearchProducts = @"
            SELECT TOP 5 ProductId, ProductName, Category
            FROM ProductMasters
            WHERE ProductName LIKE @Q
               OR Category LIKE @Q
            ORDER BY ProductId DESC";

        public const string SearchLeads = @"
            SELECT TOP 5 LeadId, LeadName, MobileNo, Status
            FROM Leads
            WHERE IsActive = 1
              AND (LeadName LIKE @Q OR MobileNo LIKE @Q)
            ORDER BY LeadId DESC";
    }
}
