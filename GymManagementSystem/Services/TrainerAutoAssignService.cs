using GymManagement.Data.Repositories;
using GymManagement.Models;

namespace GymManagement.Services
{
    public class TrainerAutoAssignService : ITrainerAutoAssignService
    {
        private readonly StaffRepository _staffRepo;
        private readonly LeadRepository _leadRepo;

        public TrainerAutoAssignService(StaffRepository staffRepo, LeadRepository leadRepo)
        {
            _staffRepo = staffRepo;
            _leadRepo = leadRepo;
        }

        public int? FindBestTrainerId(string? interestedIn)
        {
            if (string.IsNullOrWhiteSpace(interestedIn))
                return null;

            var trainers = _staffRepo.GetTrainersWithExpertise();
            if (trainers.Count == 0)
                return null;

            var interest = interestedIn.Trim();

            var best = trainers
                .Select(t => new { Trainer = t, Score = ScoreMatch(interest, t) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Trainer.ExperienceYears ?? 0)
                .ThenBy(x => x.Trainer.StaffName)
                .FirstOrDefault();

            if (best != null)
                return best.Trainer.StaffId;

            // Fallback: first active trainer if interest is set but no specialization match
            return trainers[0].StaffId;
        }

        public int? AssignTrainerToLead(int leadId, string? interestedIn)
        {
            var trainerId = FindBestTrainerId(interestedIn);
            if (!trainerId.HasValue)
                return null;

            var lead = _leadRepo.GetById(leadId);
            if (lead.LeadId <= 0)
                return null;

            if (lead.AssignedTo.HasValue && lead.AssignedTo > 0)
                return lead.AssignedTo;

            lead.AssignedTo = trainerId;
            _leadRepo.Update(lead);
            return trainerId;
        }

        public string? GetTrainerName(int trainerId)
        {
            var staff = _staffRepo.GetById(trainerId);
            return string.IsNullOrWhiteSpace(staff.StaffName) ? null : staff.StaffName;
        }

        private static int ScoreMatch(string interest, StaffMaster trainer)
        {
            var areas = ParseAreas(trainer.Specializations);
            if (areas.Count == 0)
                return 0;

            var interestLower = interest.ToLowerInvariant();
            var score = 0;

            foreach (var area in areas)
            {
                if (string.Equals(area, interest, StringComparison.OrdinalIgnoreCase))
                    score += 100;
                else if (area.Contains(interest, StringComparison.OrdinalIgnoreCase) ||
                         interest.Contains(area, StringComparison.OrdinalIgnoreCase))
                    score += 60;
            }

            if (interestLower.Contains("weight") &&
                areas.Any(a => a.Contains("weight", StringComparison.OrdinalIgnoreCase)))
                score += 40;

            if ((interestLower.Contains("muscle") || interestLower.Contains("gain")) &&
                areas.Any(a => a.Contains("muscle", StringComparison.OrdinalIgnoreCase) ||
                               a.Contains("gain", StringComparison.OrdinalIgnoreCase)))
                score += 40;

            if (interestLower.Contains("group") &&
                areas.Any(a => a.Contains("group", StringComparison.OrdinalIgnoreCase) ||
                               a.Contains("class", StringComparison.OrdinalIgnoreCase)))
                score += 40;

            if (areas.Any(a => a.Contains("personal", StringComparison.OrdinalIgnoreCase)))
                score += 15;

            score += Math.Min(trainer.ExperienceYears ?? 0, 20);
            return score;
        }

        private static List<string> ParseAreas(string? specializations)
        {
            if (string.IsNullOrWhiteSpace(specializations))
                return new List<string>();

            return specializations
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
    }
}
