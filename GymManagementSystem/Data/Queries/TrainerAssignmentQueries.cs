namespace GymManagement.Data.Queries
{
    public static class TrainerAssignmentQueries
    {
        public const string GetAll = @"
SELECT
    TA.AssignmentId,
    TA.MemberId,
    M.MemberName,
    TA.TrainerId,
    S.StaffName AS TrainerName,
    TA.StartDate,
    TA.EndDate,
    TA.Remarks,
    TA.IsActive,
    TA.CreatedDate,
    TA.CreatedBy
FROM TrainerAssignments TA
INNER JOIN MemberMasters M
    ON TA.MemberId = M.MemberId
INNER JOIN StaffMasters S
    ON TA.TrainerId = S.StaffId
ORDER BY TA.AssignmentId DESC";


        public const string GetById = @"
SELECT *
FROM TrainerAssignments
WHERE AssignmentId=@AssignmentId";


        public const string Insert = @"
INSERT INTO TrainerAssignments
(
    MemberId,
    TrainerId,
    StartDate,
    EndDate,
    Remarks,
    IsActive,
    CreatedBy,
    CreatedDate
)
VALUES
(
    @MemberId,
    @TrainerId,
    @StartDate,
    @EndDate,
    @Remarks,
    @IsActive,
    @CreatedBy,
    GETDATE()
)";


        public const string Update = @"
UPDATE TrainerAssignments
SET
    MemberId=@MemberId,
    TrainerId=@TrainerId,
    StartDate=@StartDate,
    EndDate=@EndDate,
    Remarks=@Remarks,
    IsActive=@IsActive
WHERE AssignmentId=@AssignmentId";


        public const string Delete = @"
DELETE FROM TrainerAssignments
WHERE AssignmentId=@AssignmentId";
    }
}