﻿using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.DataAccess.Repositories
{
    public class CriteriaStepRepository
    {
        private readonly BotDbContext _context;

        public CriteriaStepRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task AddOrUpdateCriteriaStepAsync(CriteriaStep criteriaStep)
        {
            var existingStep = await _context.CriteriaSteps
                .Include(cs => cs.CriteriaStepValues)
                .FirstOrDefaultAsync(cs => cs.Name == criteriaStep.Name);

            if (existingStep != null)
            {
                existingStep.Prompt = criteriaStep.Prompt;
                existingStep.IsCustom = criteriaStep.IsCustom;
                existingStep.OrderBy = criteriaStep.OrderBy;
                existingStep.Type = criteriaStep.Type;

                foreach (var value in criteriaStep.CriteriaStepValues)
                {
                    var existingValue = existingStep.CriteriaStepValues
                        .FirstOrDefault(csv => csv.Prompt == value.Prompt);

                    if (existingValue != null)
                    {
                        existingValue.Value = value.Value;
                    }
                    else
                    {
                        existingStep.CriteriaStepValues.Add(value);
                    }
                }
            }
            else
            {
                await _context.CriteriaSteps.AddAsync(criteriaStep);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<CriteriaStep>> GetAllCriteriaStepsAsync()
        {
            return await _context.CriteriaSteps
                .Include(cs => cs.CriteriaStepValues)
                .OrderBy(cs => cs.OrderBy)
                .ToListAsync();
        }

        public async Task DeleteCriteriaStepAsync(long id)
        {
            var criteriaStep = await _context.CriteriaSteps.FindAsync(id);
            if (criteriaStep != null)
            {
                _context.CriteriaSteps.Remove(criteriaStep);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<CriteriaStep> GetCriteriaStepByNameAsync(string name)
        {
            return await _context.CriteriaSteps
                .Include(cs => cs.CriteriaStepValues)
                .FirstOrDefaultAsync(cs => cs.Name == name);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}