using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class ChangePasswordStaffCommandHandler(
    IRepositoryBase<Staff, Guid> staffRepository,
    IPasswordHasherService passwordHasherService,
    IRepositoryBase<UserClinic, Guid> userClinicRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository,
    ICacheService cacheService)
    : ICommandHandler<Command.ChangePasswordStaffCommand>
{
    public async Task<Result> Handle(Command.ChangePasswordStaffCommand request, CancellationToken cancellationToken)
    {
        // Normalize email to improve matching and prevent case-sensitivity issues
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Find authenticated user (either regular user or staff)
        var staff = await staffRepository
            .FindSingleAsync(x => EF.Functions.Like(x.Email.ToLower(), normalizedEmail) && !x.IsDeleted,
                cancellationToken);
            
        if (staff is null)
            return Result.Failure(new Error("404", "User Not Found"));

        if (staff.Status == 0)
            return Result.Failure(new Error("400", "User Not Verified"));

        // Verify password and user status
        if (!passwordHasherService.VerifyPassword(request.OldPassword, staff.Password))
            return Result.Failure<dynamic>(new Error("401", "Wrong password"));
        
        var hashingPassword = passwordHasherService.HashPassword(request.NewPassword);
        staff.Password = hashingPassword;
        
        var clinicId = await userClinicRepository
            .FindAll(x => x.UserId.Equals(staff.Id))
            .Select(x => x.ClinicId)
            .FirstOrDefaultAsync(cancellationToken);

        var clinic = await clinicRepository.FindAll(x => x.Id.Equals(clinicId))
            .FirstOrDefaultAsync(cancellationToken);

        if (clinic != null)
        {
            if (request.WorkingTimeEnd != null || request.WorkingTimeStart != null)
            {
                if (request is not { WorkingTimeEnd: not null, WorkingTimeStart: not null })
                {
                    throw new Exception("WorkingTimeStart and WorkingTimeEnd must appear same !");
                }
            }
            
            if (clinic.IsFirstLogin is true)
            {
                if (request is not { WorkingTimeEnd: not null, WorkingTimeStart: not null })
                {
                    throw new Exception("WorkingTimeStart and WorkingTimeEnd must be set in the first login !");
                }

                clinic.WorkingTimeStart = request.WorkingTimeStart;
                clinic.WorkingTimeEnd = request.WorkingTimeEnd;
                clinic.IsFirstLogin = false;
                
                clinicRepository.Update(clinic);
            }
        }

        await cacheService.RemoveAsync($"Login-UserAccount:{staff.Email}", cancellationToken);

        return Result.Success("Change Password Successfully !");

    }
    
    private async Task<Result<dynamic>> FindAuthenticatedUserAsync(string normalizedEmail, string password,
        CancellationToken cancellationToken)
    {
        // Try to find user first
        var staff = await staffRepository
            .FindAll(x => EF.Functions.Like(x.Email.ToLower(), normalizedEmail) && !x.IsDeleted)
            .Select(x => new
            {
                UserId = x.Id,
                x.Email,
                x.Password,
                FullName = x.FirstName + " " + x.LastName,
                x.ProfilePicture,
                x.Status,
                x.PhoneNumber,
                Role = new
                {
                    x.Role!.Id,
                    x.Role.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (staff is null)
            return Result.Failure<dynamic>(new Error("404", "User Not Found"));

        if (staff.Status == 0)
            return Result.Failure<dynamic>(new Error("400", "User Not Verified"));

        // Verify password and user status
        return !passwordHasherService.VerifyPassword(password, staff.Password) ? Result.Failure<dynamic>(new Error("401", "Wrong password")) : Result.Success<dynamic>(staff);
    }
}