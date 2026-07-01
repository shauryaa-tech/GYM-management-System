namespace GymManagement.Data.Queries
{
    public class LeadSourceQueries
    {
        public const string GetAll = @"
SELECT *
FROM LeadSourceMasters
WHERE 1=1";

        public const string Insert = @"
INSERT INTO LeadSourceMasters
(
    SourceCode,
    SourceName,
    Description,
    DisplayOrder,
    IsActive,
    CreatedDate
)
VALUES
(
    @SourceCode,
    @SourceName,
    @Description,
    @DisplayOrder,
    @IsActive,
    GETDATE()
)";

        public const string Update = @"
UPDATE LeadSourceMasters
SET
    SourceName=@SourceName,
    Description=@Description,
    DisplayOrder=@DisplayOrder,
    IsActive=@IsActive,
    ModifiedDate=GETDATE()
WHERE LeadSourceId=@LeadSourceId";

        public const string Delete = @"
DELETE FROM LeadSourceMasters
WHERE LeadSourceId=@LeadSourceId";

        public const string GetById = @"
SELECT *
FROM LeadSourceMasters
WHERE LeadSourceId=@LeadSourceId";
    }
}