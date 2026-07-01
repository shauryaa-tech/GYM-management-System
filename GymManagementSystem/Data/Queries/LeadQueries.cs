namespace GymManagement.Data.Queries
{
    public class LeadQueries
    {
        public const string GetById = @"
SELECT *
FROM Leads
WHERE LeadId=@LeadId";

        public const string Insert = @"
INSERT INTO Leads
(
    LeadCode,
    LeadName,
    MobileNo,
    AlternateMobile,
    Email,
    Gender,
    Address,
    InterestedIn,
    LeadSourceId,
    AssignedTo,
    Status,
    Budget,
    Remarks,
    FollowUpDate,
    IsConverted,
    IsActive,
    CreatedDate
)
VALUES
(
    @LeadCode,
    @LeadName,
    @MobileNo,
    @AlternateMobile,
    @Email,
    @Gender,
    @Address,
    @InterestedIn,
    @LeadSourceId,
    @AssignedTo,
    @Status,
    @Budget,
    @Remarks,
    @FollowUpDate,
    @IsConverted,
    @IsActive,
    GETDATE()
)";

        public const string InsertBasic = @"
INSERT INTO Leads
(
    LeadName,
    MobileNo,
    Email,
    InterestedIn,
    LeadSourceId,
    AssignedTo,
    Status,
    Remarks,
    FollowUpDate,
    CreatedDate
)
VALUES
(
    @LeadName,
    @MobileNo,
    @Email,
    @InterestedIn,
    @LeadSourceId,
    @AssignedTo,
    @Status,
    @Remarks,
    @FollowUpDate,
    GETDATE()
)";

        public const string Update = @"
UPDATE Leads
SET
LeadName=@LeadName,
MobileNo=@MobileNo,
AlternateMobile=@AlternateMobile,
Email=@Email,
Gender=@Gender,
Address=@Address,
InterestedIn=@InterestedIn,
LeadSourceId=@LeadSourceId,
AssignedTo=@AssignedTo,
Status=@Status,
Budget=@Budget,
Remarks=@Remarks,
FollowUpDate=@FollowUpDate,
IsConverted=@IsConverted,
IsActive=@IsActive,
ModifiedDate=GETDATE()
WHERE LeadId=@LeadId";

        public const string UpdateBasic = @"
UPDATE Leads
SET
LeadName=@LeadName,
MobileNo=@MobileNo,
Email=@Email,
InterestedIn=@InterestedIn,
LeadSourceId=@LeadSourceId,
AssignedTo=@AssignedTo,
Status=@Status,
Remarks=@Remarks,
FollowUpDate=@FollowUpDate
WHERE LeadId=@LeadId";

        public const string Delete = @"
DELETE FROM Leads
WHERE LeadId=@LeadId";

        public const string GetLatestInterestedInByMobile = @"
        SELECT TOP 1 InterestedIn
        FROM Leads
        WHERE MobileNo = @MobileNo
          AND InterestedIn IS NOT NULL
          AND LTRIM(RTRIM(InterestedIn)) <> ''
        ORDER BY LeadId DESC";
    }
}
