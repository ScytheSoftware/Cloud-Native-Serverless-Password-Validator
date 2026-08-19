using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api
{
    // 1. Data Transfer Objects (DTOs) for strict request/response contracts
    public class PasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class ValidationChecks
    {
        public bool MinLengthPassed { get; set; }
        public bool MaxLengthPassed { get; set; }
        public bool HasNumber { get; set; }
        public bool HasSpecialChar { get; set; }
        public bool NoSpaces { get; set; }
    }

    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public ValidationChecks Checks { get; set; } = new();
    }

    public class ValidatePassword
    {
        private readonly ILogger<ValidatePassword> _logger;
        
        // Your original 2020 special characters array
        private static readonly char[] SpecialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '?', '_', '+' };

        public ValidatePassword(ILogger<ValidatePassword> logger)
        {
            _logger = logger;
        }

        [Function("ValidatePassword")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing a password validation request.");

            // 2. Read and parse the incoming JSON payload asynchronously
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body cannot be empty." });
            }

            PasswordRequest? data;
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                data = JsonSerializer.Deserialize<PasswordRequest>(requestBody, options);
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult(new { error = "Invalid JSON payload." });
            }

            string password = data?.Password ?? string.Empty;

            // 3. Modernized Logic Core (Your rules written as fast, stateless LINQ evaluations)
            var checks = new ValidationChecks
            {
                MinLengthPassed = password.Length >= 8,
                MaxLengthPassed = password.Length <= 15,
                HasNumber = password.Any(char.IsDigit),
                HasSpecialChar = password.Any(c => SpecialCharacters.Contains(c)),
                NoSpaces = !password.Contains(" ")
            };

            // Overall status is true ONLY if every single checklist item is true
            bool isValid = checks.MinLengthPassed && 
                           checks.MaxLengthPassed && 
                           checks.HasNumber && 
                           checks.HasSpecialChar && 
                           checks.NoSpaces;

            // 4. Secure Compliance Logging: Log the results but NEVER log the password value
            _logger.LogInformation("Validation finished. Overall Valid: {IsValid}. Failures - Length: {LengthFail}, Num: {NumFail}, Special: {SpecialFail}, Spaces: {SpaceFail}",
                isValid,
                (!checks.MinLengthPassed || !checks.MaxLengthPassed),
                !checks.HasNumber,
                !checks.HasSpecialChar,
                !checks.NoSpaces);

            // 5. Structure the return payload
            var response = new ValidationResponse
            {
                IsValid = isValid,
                Checks = checks
            };

            return new OkObjectResult(response);
        }
    }
}