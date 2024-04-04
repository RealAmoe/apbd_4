using System;

namespace LegacyApp
{
    public interface IUserCreditService
    {
        int GetCreditLimit(string lastName, DateTime dateOfBirth);
    }

    public interface IClientRepository
    {
        Client GetById(int clientId);
    }

    public class UserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserCreditService _userCreditService;

        // Parameterless constructor for backward compatibility
        public UserService() 
            : this(new ClientRepository(), new UserCreditService())
        {
        }

        // Constructor that allows dependency injection
        public UserService(IClientRepository clientRepository, IUserCreditService userCreditService)
        {
            _clientRepository = clientRepository;
            _userCreditService = userCreditService;
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidUserInput(firstName, lastName, email, dateOfBirth))
                return false;

            var client = _clientRepository.GetById(clientId);
            var user = new User(client, dateOfBirth, email, firstName, lastName);

            DetermineCreditLimit(user, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidUserInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            return !string.IsNullOrEmpty(firstName) && 
                   !string.IsNullOrEmpty(lastName) && 
                   email.Contains("@") && 
                   email.Contains(".") && 
                   CalculateAge(dateOfBirth) >= 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;
            return age;
        }
        //
        // private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        // {
        //     return new User
        //     {
        //         Client = client,
        //         DateOfBirth = dateOfBirth,
        //         EmailAddress = email,
        //         FirstName = firstName,
        //         LastName = lastName
        //     };
        // }

        
        
        private void DetermineCreditLimit(User user, Client client)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    user.HasCreditLimit = true;
                    SetCreditLimit(user, 2);
                    break;
                default:
                    user.HasCreditLimit = true;
                    SetCreditLimit(user, 1);
                    break;
            }
        }

        private void SetCreditLimit(User user, int multiplier)
        {
            int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
            user.CreditLimit = creditLimit * multiplier;
        }
    }
}
