using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class MatchService
{
    private readonly SqlMatchRepository _matchRepository;
    private readonly JobService _jobService;

    public MatchService(SqlMatchRepository matchRepository, JobService jobService)
    {
        _matchRepository = matchRepository;
        _jobService = jobService;
    }

    public Match? GetById(int matchId) => _matchRepository.GetById(matchId);

    public Match? GetByUserIdAndJobId(int userId, int jobId) =>
        _matchRepository.GetByUserIdAndJobId(userId, jobId);

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

        return _matchRepository.InsertReturningId(match);
    }

    public void RemoveApplication(int matchId) => _matchRepository.Remove(matchId);

    public IReadOnlyList<Match> GetAllMatches() => _matchRepository.GetAll();

    public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId)
    {
        var companyJobIds = _jobService
            .GetByCompanyId(companyId)
            .Select(job => job.JobId)
            .ToHashSet();

        if (companyJobIds.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<Match>>([]);
        }

        var matches = _matchRepository
            .GetAll()
            .Where(match => companyJobIds.Contains(match.JobId))
            .OrderByDescending(match => match.Timestamp)
            .ToList();

        return Task.FromResult<IReadOnlyList<Match>>(matches);
    }

    public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback)
    {
        var match = _matchRepository.GetById(matchId)
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
        _matchRepository.Update(match);

        return Task.CompletedTask;
    }

    public Task AcceptAsync(int matchId, string feedback)
    {
        return SubmitDecisionAsync(matchId, MatchStatus.Accepted, feedback);
    }

    public Task RejectAsync(int matchId, string feedback)
    {
        return SubmitDecisionAsync(matchId, MatchStatus.Rejected, feedback);
    }

    // TODO: Update UML (.mdj) to include Advance(int matchId) in MatchService.
    public void Advance(int matchId)
    {
        var match = _matchRepository.GetById(matchId)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        if (match.Status != MatchStatus.Applied)
        {
            throw new InvalidOperationException(
                $"Cannot advance match {matchId}: status is {match.Status}, expected Applied.");
        }

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        _matchRepository.Update(match);
    }

    public void RevertToApplied(int matchId)
    {
        var match = _matchRepository.GetById(matchId)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        match.Status = MatchStatus.Applied;
        match.FeedbackMessage = string.Empty;
        match.Timestamp = DateTime.UtcNow;
        _matchRepository.Update(match);
    }

    public bool IsDecisionTransitionAllowed(Match current, MatchStatus next)
    {
        if (current.Status != MatchStatus.Applied)
        {
            return false;
        }

        return next is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
    }

    private static void ValidateDecisionInput(MatchStatus decision, string feedback)
    {
        if (decision is not (MatchStatus.Accepted or MatchStatus.Rejected))
        {
            throw new ArgumentException("Decision must be either Accepted or Rejected.", nameof(decision));
        }

        if (decision == MatchStatus.Rejected && string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException("Feedback is required when rejecting an applicant.", nameof(feedback));
        }
    }
}
