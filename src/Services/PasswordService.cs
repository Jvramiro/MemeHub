using System.Text.RegularExpressions;

namespace MemeHub.Services {
    public class PasswordService {

        public static bool CheckStrength(string password) {

            bool isWeak = password.Length < 8
                            //|| !Regex.IsMatch(password, "[A-Z]")
                            //|| !Regex.IsMatch(password, "[a-z]")
                            //|| !Regex.IsMatch(password, "[0-9]")
                            //|| !Regex.IsMatch(password, "[!@#$%^&*()]")
                            ;

            return !isWeak;

        }

    }
}
