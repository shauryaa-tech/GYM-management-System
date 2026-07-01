namespace GymManagement.Data.Queries
{
    public static class MembershipQueries
    {
        public const string GetAll =
            "SELECT * FROM MembershipPlanMasters ORDER BY PlanId DESC";

        public const string GetById =
            "SELECT * FROM MembershipPlanMasters WHERE PlanId=@PlanId";

        public const string Insert = @"
            INSERT INTO MembershipPlanMasters
            (
                PlanName,
                DurationMonths,
                Amount,
                JoiningFee,
                Description,
                IsActive
            )
            VALUES
            (
                @PlanName,
                @DurationMonths,
                @Amount,
                @JoiningFee,
                @Description,
                @IsActive
            )";

        public const string Update = @"
            UPDATE MembershipPlanMasters
            SET
                PlanName=@PlanName,
                DurationMonths=@DurationMonths,
                Amount=@Amount,
                JoiningFee=@JoiningFee,
                Description=@Description,
                IsActive=@IsActive
            WHERE PlanId=@PlanId";

        public const string Delete =
            "DELETE FROM MembershipPlanMasters WHERE PlanId=@PlanId";
    }
}