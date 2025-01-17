// using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Entities;
//
// namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;
//
// public class RegisterCommandHandler : ICommandHandler<Command.RegisterCommand>
// {
//     private readonly IRepositoryBase<User, Guid> _userRepository;
//     private readonly IPasswordHasherService _passwordHasherService;
//
//     public RegisterCommandHandler(IRepositoryBase<User, Guid> userRepository, IPasswordHasherService passwordHasherService)
//     {
//         _userRepository = userRepository;
//         _passwordHasherService = passwordHasherService;
//     }
//
//     public async Task<Result> Handle(Command.RegisterCommand request, CancellationToken cancellationToken)
//     {
//         var userExisted =
//             await _userRepository.FindSingleAsync(x =>
//                 x.Email.Equals(request.Email), cancellationToken);
//         
//         if (userExisted is not null)
//         {
//             throw new Exception("User Existed !");
//         }
//
//         var hashingPassword = _passwordHasherService.HashPassword(request.Password);
//         
//         var user = new User()
//         {
//             // Id = Guid.NewGuid(),
//             // Email = request.Email,
//             // FullName = request.FirstName + request.LastName,
//             // Role = request.Role,
//             //
//             // Password = hashingPassword,
//             //
//             // Points = 0,
//             // Status = 0
//         };
//         
//         _userRepository.Add(user);
//
//         return Result.Success(user);
//     }
// }