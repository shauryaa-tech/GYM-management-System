namespace GymManagement.Data.Queries
{
    public static class ClassBookingQueries
    {
        public const string GetAll = @"
        SELECT 
            CB.*,
            M.MemberName,
            M.MemberCode,
            CM.ClassName,
            CM.StartTime,
            CM.EndTime,
            S.StaffName AS TrainerName
        FROM ClassBookings CB
        LEFT JOIN MemberMasters M ON CB.MemberId = M.MemberId
        LEFT JOIN ClassMasters CM ON CB.ClassId = CM.ClassId
        LEFT JOIN StaffMasters S ON CM.TrainerId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT 
            CB.*,
            M.MemberName,
            M.MemberCode,
            CM.ClassName,
            CM.StartTime,
            CM.EndTime,
            S.StaffName AS TrainerName
        FROM ClassBookings CB
        LEFT JOIN MemberMasters M ON CB.MemberId = M.MemberId
        LEFT JOIN ClassMasters CM ON CB.ClassId = CM.ClassId
        LEFT JOIN StaffMasters S ON CM.TrainerId = S.StaffId
        WHERE CB.BookingId=@BookingId";

        public const string Insert = @"
        INSERT INTO ClassBookings
        (
            MemberId,
            ClassId,
            BookingDate,
            Status,
            Amount,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @ClassId,
            @BookingDate,
            @Status,
            @Amount,
            @Remarks
        )";

        public const string Update = @"
        UPDATE ClassBookings
        SET
            MemberId=@MemberId,
            ClassId=@ClassId,
            BookingDate=@BookingDate,
            Status=@Status,
            Amount=@Amount,
            Remarks=@Remarks
        WHERE BookingId=@BookingId";

        public const string Delete = "DELETE FROM ClassBookings WHERE BookingId=@BookingId";

        public const string GetBookingsByClass = @"
        SELECT 
            CB.*,
            M.MemberName,
            M.MemberCode,
            CM.ClassName,
            CM.StartTime,
            CM.EndTime,
            S.StaffName AS TrainerName
        FROM ClassBookings CB
        LEFT JOIN MemberMasters M ON CB.MemberId = M.MemberId
        LEFT JOIN ClassMasters CM ON CB.ClassId = CM.ClassId
        LEFT JOIN StaffMasters S ON CM.TrainerId = S.StaffId
        WHERE CB.ClassId=@ClassId AND CB.BookingDate=@BookingDate";

        public const string GetMemberBookings = @"
        SELECT 
            CB.*,
            M.MemberName,
            M.MemberCode,
            CM.ClassName,
            CM.StartTime,
            CM.EndTime,
            S.StaffName AS TrainerName
        FROM ClassBookings CB
        LEFT JOIN MemberMasters M ON CB.MemberId = M.MemberId
        LEFT JOIN ClassMasters CM ON CB.ClassId = CM.ClassId
        LEFT JOIN StaffMasters S ON CM.TrainerId = S.StaffId
        WHERE CB.MemberId=@MemberId";

        public const string GetBookingCountByClass = @"
        SELECT COUNT(*) FROM ClassBookings
        WHERE ClassId=@ClassId AND BookingDate=@BookingDate AND Status='Confirmed'";

        public const string GetActiveClassesForBooking = @"
        SELECT ClassId, ClassName, TrainerId, Schedule, StartTime, EndTime, MaxCapacity, Amount
        FROM ClassMasters
        WHERE IsActive=1";
    }
}