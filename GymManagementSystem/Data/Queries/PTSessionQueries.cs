namespace GymManagement.Data.Queries
{
    public static class PTSessionQueries
    {
        public const string GetAll = @"
        SELECT 
            P.*,
            M.MemberName,
            M.MemberCode,
            S.StaffName AS TrainerName
        FROM PTSessions P
        INNER JOIN MemberMasters M ON P.MemberId = M.MemberId
        INNER JOIN StaffMasters S ON P.TrainerId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM PTSessions
        WHERE SessionId=@SessionId";

        public const string Insert = @"
        INSERT INTO PTSessions
        (
            MemberId,
            TrainerId,
            SessionDate,
            StartTime,
            EndTime,
            Status,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @TrainerId,
            @SessionDate,
            @StartTime,
            @EndTime,
            @Status,
            @Remarks
        )";

        public const string Update = @"
        UPDATE PTSessions
        SET
            MemberId=@MemberId,
            TrainerId=@TrainerId,
            SessionDate=@SessionDate,
            StartTime=@StartTime,
            EndTime=@EndTime,
            Status=@Status,
            Remarks=@Remarks
        WHERE SessionId=@SessionId";

        public const string Delete =
            "DELETE FROM PTSessions WHERE SessionId=@SessionId";
    }
}
