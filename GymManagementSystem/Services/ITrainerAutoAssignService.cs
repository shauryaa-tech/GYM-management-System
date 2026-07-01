namespace GymManagement.Services
{
    public interface ITrainerAutoAssignService
    {
        int? FindBestTrainerId(string? interestedIn);

        int? AssignTrainerToLead(int leadId, string? interestedIn);

        string? GetTrainerName(int trainerId);
    }
}
