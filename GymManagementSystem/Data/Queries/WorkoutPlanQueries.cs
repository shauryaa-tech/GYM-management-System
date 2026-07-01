namespace GymManagement.Data.Queries
{
    public static class WorkoutPlanQueries
    {
        public const string GetAll = @"
        SELECT 
            WP.*,
            M.MemberName,
            M.MemberCode,
            S.StaffName AS TrainerName
        FROM WorkoutPlans WP
        INNER JOIN MemberMasters M ON WP.MemberId = M.MemberId
        LEFT JOIN StaffMasters S ON WP.TrainerId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM WorkoutPlans
        WHERE PlanId=@PlanId";

        public const string Insert = @"
        INSERT INTO WorkoutPlans
        (
            MemberId,
            TrainerId,
            PlanName,
            StartDate,
            EndDate,
            Goals,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @TrainerId,
            @PlanName,
            @StartDate,
            @EndDate,
            @Goals,
            @Remarks
        )";

        public const string Update = @"
        UPDATE WorkoutPlans
        SET
            MemberId=@MemberId,
            TrainerId=@TrainerId,
            PlanName=@PlanName,
            StartDate=@StartDate,
            EndDate=@EndDate,
            Goals=@Goals,
            Remarks=@Remarks
        WHERE PlanId=@PlanId";

        public const string Delete =
            "DELETE FROM WorkoutPlans WHERE PlanId=@PlanId";
    }
}
