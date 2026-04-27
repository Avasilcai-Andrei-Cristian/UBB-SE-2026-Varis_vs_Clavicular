using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository matchRepository;
    private readonly IJobService jobService;

    public MatchService(IMatchRepository matchRepository, IJobService jobService)
    {
        this.matchRepository = matchRepository;
        this.jobService = jobService;
    }

    public Match? GetById(int matchId) => matchRepository.GetById(matchId);

    public Match? GetByUserIdAndJobId(int userId, int jobId) =>
        matchRepository.GetByUserIdAndJobId(userId, jobId);

    public int CreatePendingApplication(int userId, int jobId)
    {
        if (GetByUserIdAndJobId(userId, jobId) is not null)
        {
            throw new InvalidOperationException("A match already exists for this user and job.");
        }

        var match = new Match
        {
            UserId = userId,
            JobId = jobId,
            Status = MatchStatus.Applied,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = string.Empty
        };

        return matchRepository.InsertReturningId(match);
    }

    public void RemoveApplication(int matchId) => matchRepository.Remove(matchId);

    public IReadOnlyList<Match> GetAllMatches() => matchRepository.GetAll();

    public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId)
    {
        var companyJobIds = new HashSet<int>();
        foreach (var job in jobService.GetByCompanyId(companyId))
        {
            companyJobIds.Add(job.JobId);
        }

        if (companyJobIds.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<Match>>([]);
        }

        var matches = new List<Match>();
        foreach (var match in matchRepository.GetAll())
        {
            if (companyJobIds.Contains(match.JobId))
            {
                matches.Add(match);
            }
        }

        matches.Sort(CompareByTimestampDescending);

        return Task.FromResult<IReadOnlyList<Match>>(matches);
    }

    public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback)
    {
        SubmitDecision(matchId, decision, feedback);
        return Task.CompletedTask;
    }

    public void SubmitDecision(int matchId, MatchStatus decision, string feedback)
    {
        var match = matchRepository.GetById(matchId)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        ValidateDecisionInput(decision, feedback);

        if (!IsDecisionTransitionAllowed(match, decision))
        {
            throw new InvalidOperationException(
                $"Cannot change match {matchId} status from {match.Status} to {decision}.");
        }

        match.Status = decision;
        match.FeedbackMessage = feedback.Trim();
        match.Timestamp = DateTime.UtcNow;
        matchRepository.Update(match);
    }

    public Task AcceptAsync(int matchId, string feedback)
    {
        SubmitDecision(matchId, MatchStatus.Accepted, feedback);
        return Task.CompletedTask;
    }

    public Task RejectAsync(int matchId, string feedback)
    {
        SubmitDecision(matchId, MatchStatus.Rejected, feedback);
        return Task.CompletedTask;
    }

    public void Reject(int matchId, string feedback)
    {
        SubmitDecision(matchId, MatchStatus.Rejected, feedback);
    }
    public void Advance(int matchId)
    {
        var match = matchRepository.GetById(matchId)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        if (match.Status != MatchStatus.Applied)
        {
            throw new InvalidOperationException(
                $"Cannot advance match {matchId}: status is {match.Status}, expected Applied.");
        }

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        matchRepository.Update(match);
    }

    public void RevertToApplied(int matchId)
    {
        var match = matchRepository.GetById(matchId)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        match.Status = MatchStatus.Applied;
        match.FeedbackMessage = string.Empty;
        match.Timestamp = DateTime.UtcNow;
        matchRepository.Update(match);
    }

    public bool IsDecisionTransitionAllowed(Match current, MatchStatus next)
    {
        if (current.Status == MatchStatus.Applied)
        {
            return next is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
        }

        if (current.Status == MatchStatus.Advanced)
        {
            return next is MatchStatus.Accepted or MatchStatus.Rejected;
        }

        return false;
    }

    private static void ValidateDecisionInput(MatchStatus decision, string feedback)
    {
        if (decision != MatchStatus.Accepted && decision != MatchStatus.Rejected)
        {
            throw new ArgumentException("Decision must be either Accepted or Rejected.", nameof(decision));
        }

        if (decision == MatchStatus.Rejected && string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException("Feedback is required when rejecting an applicant.", nameof(feedback));
        }
    }

    private static int CompareByTimestampDescending(Match left, Match right)
    {
        return right.Timestamp.CompareTo(left.Timestamp);
    }
}
