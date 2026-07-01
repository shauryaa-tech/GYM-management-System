namespace GymManagement.Data.Queries
{
    public static class MemberQueries
    {
        public const string GetAll = @"
                SELECT
            M.*,
            S.StaffName AS TrainerName,
            P.PlanName
        FROM MemberMasters M

        LEFT JOIN StaffMasters S
        ON M.TrainerId = S.StaffId

        LEFT JOIN MembershipPlanMasters P
        ON M.PlanId = P.PlanId";

        public const string GetById = @"
        SELECT * FROM MemberMasters
        WHERE MemberId=@MemberId";

        public const string Insert = @"
        INSERT INTO MemberMasters
        (
            MemberCode,
            MemberName,
            MobileNo,
            AlternateMobile,
            Email,
            Gender,
            DateOfBirth,
            BloodGroup,
            Address,
            City,
            State,
            Pincode,
            EmergencyContact,
            EmergencyContactName,
            TrainerId,
            PlanId,
            JoinDate,
            PlanStartDate,
            PlanEndDate,
            Height,
            Weight,
            Status,
            MedicalNotes,
            Remarks
        )
        VALUES
        (
            @MemberCode,
            @MemberName,
            @MobileNo,
            @AlternateMobile,
            @Email,
            @Gender,
            @DateOfBirth,
            @BloodGroup,
            @Address,
            @City,
            @State,
            @Pincode,
            @EmergencyContact,
            @EmergencyContactName,
            @TrainerId,
            @PlanId,
            @JoinDate,
            @PlanStartDate,
            @PlanEndDate,
            @Height,
            @Weight,
            @Status,
            @MedicalNotes,
            @Remarks
        )";

        public const string Update = @"
        UPDATE MemberMasters
        SET
            MemberName=@MemberName,
            MobileNo=@MobileNo,
            AlternateMobile=@AlternateMobile,
            Email=@Email,
            Gender=@Gender,
            DateOfBirth=@DateOfBirth,
            BloodGroup=@BloodGroup,
            Address=@Address,
            City=@City,
            State=@State,
            Pincode=@Pincode,
            EmergencyContact=@EmergencyContact,
            EmergencyContactName=@EmergencyContactName,
            TrainerId=@TrainerId,
            PlanId=@PlanId,
            JoinDate=@JoinDate,
            PlanStartDate=@PlanStartDate,
            PlanEndDate=@PlanEndDate,
            Height=@Height,
            Weight=@Weight,
            Status=@Status,
            MedicalNotes=@MedicalNotes,
            Remarks=@Remarks
            ModifiedDate=GETDATE()
        WHERE MemberId=@MemberId";

        public const string Delete =
            "DELETE FROM MemberMasters WHERE MemberId=@MemberId";
    }
}