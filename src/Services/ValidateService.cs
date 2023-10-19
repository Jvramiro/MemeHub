using MemeHub.Models;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MemeHub.Services {
    public class ValidateService {

        public static bool ValidateUser(User user, out IResult result) {

            result = !ValitadeEmail(user.Email) ?
                        Results.BadRequest("Invalid Email") :
                    Results.Ok();

            return result == Results.Ok();

        }

        public static bool ValitadeEmail(string email) {
            return new EmailAddressAttribute().IsValid(email);
        }

    }
}
