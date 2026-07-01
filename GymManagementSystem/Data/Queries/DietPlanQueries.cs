namespace GymManagement.Data.Queries
{
    public static class DietPlanQueries
    {
        public const string GetAll = @"
        SELECT 
            DP.*,
            M.MemberName,
            M.MemberCode
        FROM DietPlans DP
        INNER JOIN MemberMasters M ON DP.MemberId = M.MemberId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM DietPlans
        WHERE DietPlanId=@DietPlanId";

        public const string Insert = @"
        INSERT INTO DietPlans
        (
            MemberId,
            PlanName,
            StartDate,
            EndDate,
            CalorieTarget,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @PlanName,
            @StartDate,
            @EndDate,
            @CalorieTarget,
            @Remarks
        )";

        public const string Update = @"
        UPDATE DietPlans
        SET
            MemberId=@MemberId,
            PlanName=@PlanName,
            StartDate=@StartDate,
            EndDate=@EndDate,
            CalorieTarget=@CalorieTarget,
            Remarks=@Remarks
        WHERE DietPlanId=@DietPlanId";

        public const string Delete =
            "DELETE FROM DietPlans WHERE DietPlanId=@DietPlanId";
    }
}
