using System.Text.RegularExpressions;

namespace Baxture.Api.Dtos;

public static partial class UserRequestValidator
{
    public static List<string> ValidateCreate(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors.Add("username is required.");
        }

        ValidatePassword(request.Password, true, errors);

        if (request.Age is null)
        {
            errors.Add("age is required.");
        }
        else if (request.Age < 0)
        {
            errors.Add("age must be zero or greater.");
        }

        if (request.Hobbies is null)
        {
            errors.Add("hobbies is required and can be an empty array.");
        }

        return errors;
    }

    public static List<string> ValidateUpdate(UpdateUserRequest request)
    {
        var errors = new List<string>();

        if (request.Username is not null && string.IsNullOrWhiteSpace(request.Username))
        {
            errors.Add("username cannot be empty.");
        }

        ValidatePassword(request.Password, false, errors);

        if (request.Age is < 0)
        {
            errors.Add("age must be zero or greater.");
        }

        return errors;
    }

    private static void ValidatePassword(string? password, bool required, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if (required)
            {
                errors.Add("password is required.");
            }

            return;
        }

        if (!AlphanumericRegex().IsMatch(password))
        {
            errors.Add("password must be alphanumeric.");
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex AlphanumericRegex();
}
