namespace GymManagement.Data.Queries
{
    public static class MembersQueries
    {
        public const string GetAll = @"
        SELECT
            M.MemberId,
            M.MemberCode,
            M.MemberName,
            M.MobileNo,
            M.Status,
            M.JoinDate,

            S.StaffName AS TrainerName,

            MP.PlanName

        FROM MemberMasters M

        LEFT JOIN TrainerAssignments TA
        ON M.MemberId = TA.MemberId
        AND TA.IsActive = 1

        LEFT JOIN StaffMasters S
        ON TA.TrainerId = S.StaffId

        LEFT JOIN MembershipPlanMasters MP
        ON M.PlanId = MP.PlanId";


        public const string GetById = @"
        SELECT

        M.*,

        S.StaffName AS TrainerName,

        MP.PlanName

        FROM MemberMasters M

        LEFT JOIN TrainerAssignments TA
        ON M.MemberId = TA.MemberId
        AND TA.IsActive = 1

        LEFT JOIN StaffMasters S
        ON TA.TrainerId = S.StaffId

        LEFT JOIN MembershipPlanMasters MP
        ON M.PlanId=MP.PlanId

        WHERE M.MemberId=@MemberId";
    }
}