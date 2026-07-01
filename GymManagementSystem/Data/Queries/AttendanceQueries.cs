namespace GymManagement.Data.Queries
{
    public static class AttendanceQueries
    {
        public const string GetAll = @"
        SELECT 
            A.*,
            M.MemberName,
            M.MemberCode
        FROM Attendances A
        INNER JOIN MemberMasters M ON A.MemberId = M.MemberId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM Attendances
        WHERE AttendanceId=@AttendanceId";

        public const string Insert = @"
        INSERT INTO Attendances
        (
            MemberId,
            AttendanceDate,
            CheckInTime,
            CheckOutTime,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @AttendanceDate,
            @CheckInTime,
            @CheckOutTime,
            @Remarks
        )";

        public const string Update = @"
        UPDATE Attendances
        SET
            MemberId=@MemberId,
            AttendanceDate=@AttendanceDate,
            CheckInTime=@CheckInTime,
            CheckOutTime=@CheckOutTime,
            Remarks=@Remarks
        WHERE AttendanceId=@AttendanceId";

        public const string Delete =
            "DELETE FROM Attendances WHERE AttendanceId=@AttendanceId";

        public const string GetByMemberDateRange = @"
        SELECT 
            A.*,
            M.MemberName,
            M.MemberCode
        FROM Attendances A
        INNER JOIN MemberMasters M ON A.MemberId = M.MemberId
        WHERE A.MemberId = @MemberId
          AND A.AttendanceDate >= @FromDate
          AND A.AttendanceDate <= @ToDate
        ORDER BY A.AttendanceDate DESC";
    }
}
