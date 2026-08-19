// "using" statements are imports. They bring in external toolboxes (namespaces) 
// containing pre-built code classes so we don't have to write everything from scratch.
using System.IO;                  // Toolbox for reading data streams (incoming network files/web requests)
using System.Linq;                // "Language Integrated Query" - Toolbox for high-performance data matching & filtering
using System.Text.Json;          // Toolbox for translating JSON text into structured C# variables
using System.Threading.Tasks;     // Toolbox for handling asynchronous (non-blocking) multi-threaded tasks
using Microsoft.AspNetCore.Http;  // Toolbox for handling standard web traffic parameters (HTTP requests/responses)
using Microsoft.AspNetCore.Mvc;   // Toolbox containing standard web API responses (like "200 OK" or "400 Bad Request")
using Microsoft.Azure.Functions.Worker; // Core Azure engine toolbox for executing serverless function triggers
using Microsoft.Extensions.Logging;    // Standard logging toolbox for recording secure system activities

namespace api
{
    // =================================================================================
    // SECTION 1: DATA TRANSFER OBJECTS (DTOs)
    // In web APIs, we use DTOs as explicit contracts or templates. They guarantee the 
    // exact structure of data coming in (Requests) and data going out (Responses).
    // =================================================================================

    /// <summary>
    /// This template represents the incoming JSON "envelope" sent by the user's browser.
    /// Expects data formatted as: { "password": "UserTypedPassword" }
    /// </summary>
    public class PasswordRequest
    {
        // "get; set;" allows read/write access to this property. 
        // We initialize it as empty to prevent "null reference" crashes.
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// This template holds the boolean (true/false) results for each individual security rule.
    /// This allows our frontend website to dynamically color rules green (passed) or red (failed).
    /// </summary>
    public class ValidationChecks
    {
        public bool MinLengthPassed { get; set; } // True if password is 8 characters or more
        public bool MaxLengthPassed { get; set; } // True if password is 15 characters or less
        public bool HasNumber { get; set; }        // True if password contains at least one digit (0-9)
        public bool HasSpecialChar { get; set; }   // True if password contains a required symbol (like !, @, #)
        public bool NoSpaces { get; set; }         // True if password contains no spaces
    }

    /// <summary>
    /// This template represents the final JSON payload returned back to the user's browser.
    /// It bundles the overall status with the granular rule-by-rule checklist.
    /// </summary>
    public class ValidationResponse
    {
        public bool IsValid { get; set; } // Overall system grade: True only if ALL rules passed
        public ValidationChecks Checks { get; set; } = new(); // Instantiates the granular check list above
    }

    // =================================================================================
    // SECTION 2: THE SERVERLESS CONTROLLER ENGINE
    // This is the active blueprint class that handles incoming network traffic.
    // =================================================================================
    public class ValidatePassword
    {
        // We create a private, read-only slot to hold our system logger (the security notepad)
        private readonly ILogger<ValidatePassword> _logger;
        
        // This array defines our valid special characters, exactly matching your 2020 application.
        // Declared as "static readonly" so it is loaded once into system memory and shared safely.
        private static readonly char[] SpecialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '?', '_', '+' };

        /// <summary>
        /// Constructor: This initializes our function class.
        /// We use Dependency Injection (DI) to pass the Logger service into our class.
        /// </summary>
        public ValidatePassword(ILogger<ValidatePassword> logger)
        {
            _logger = logger; // Binds the incoming logger service to our local private field
        }

        /// <summary>
        /// The main execution method. Whenever a network request hits our API URL, Azure wakes up
        /// this method, processes the payload, and puts the thread back to sleep.
        /// </summary>
        // [Function] tells the Azure runtime engine: "Expose this method as a cloud endpoint named ValidatePassword"
        [Function("ValidatePassword")]
        public async Task<IActionResult> Run(
            // [HttpTrigger] tells Azure to execute this code when it receives an HTTP POST request.
            // "AuthorizationLevel.Anonymous" means anyone on the web can call this API without an access key.
            // "HttpRequest req" represents the incoming web request envelope.
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Write a marker in our Azure console logs showing we have received a request
            _logger.LogInformation("Processing a password validation request.");

            // -------------------------------------------------------------------------
            // STEP A: EXTRACT THE INCOMING WEB PAYLOAD
            // Web requests arrive as digital streams (binary streams of raw text). We must
            // read this stream and translate it into a structured C# object.
            // -------------------------------------------------------------------------

            // StreamReader works like an open bucket catching water from a pipe. 
            // It reads the incoming request body from start to finish asynchronously.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            // Defensive Programming: If the body is completely empty, reject the request immediately.
            if (string.IsNullOrEmpty(requestBody))
            {
                // Returns an HTTP 400 Bad Request indicating the client sent an empty payload
                return new BadRequestObjectResult(new { error = "Request body cannot be empty." });
            }

            PasswordRequest? data;
            try
            {
                // We set options to translate "camelCase" JSON (web standard) to C# "PascalCase" seamlessly.
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                
                // Deserialize translates raw JSON text: {"password": "Test"} into a physical C# Object: PasswordRequest
                data = JsonSerializer.Deserialize<PasswordRequest>(requestBody, options);
            }
            catch (JsonException)
            {
                // If the user sends broken or malformed JSON text, reject it as an HTTP 400 Bad Request
                return new BadRequestObjectResult(new { error = "Invalid JSON payload format." });
            }

            // Extract the password string. If the object was null, fallback safely to an empty string.
            string password = data?.Password ?? string.Empty;

            // -------------------------------------------------------------------------
            // STEP B: EXECUTE THE STATELESS SECURITY CHECKS
            // We evaluate your rules using LINQ (Language Integrated Query) expressions.
            // This is high-performance, stateless, and thread-safe.
            // -------------------------------------------------------------------------
            var checks = new ValidationChecks
            {
                // Rule 1: Minimum Length Check (Must be 8 characters or more)
                MinLengthPassed = password.Length >= 8,

                // Rule 2: Maximum Length Check (Must be 15 characters or less)
                MaxLengthPassed = password.Length <= 15,

                // Rule 3: Number Verification Check
                // "password.Any(char.IsDigit)" uses a built-in character validator. It iterates
                // through the string. The moment it detects any character (0-9), it stops and returns true.
                HasNumber = password.Any(char.IsDigit),

                // Rule 4: Special Character Verification Check
                // "c => SpecialCharacters.Contains(c)" is a lambda (shorthand callback function).
                // It asks: "Are there ANY characters 'c' in this password that are inside our SpecialCharacters list?"
                HasSpecialChar = password.Any(c => SpecialCharacters.Contains(c)),

                // Rule 5: Spaces Verification Check
                // Negating the result: "!password.Contains(' ')" means we pass ONLY if no spaces are detected.
                NoSpaces = !password.Contains(" ")
            };

            // Calculate the overall grade: Must be true ONLY if all 5 checks successfully passed.
            bool isValid = checks.MinLengthPassed && 
                           checks.MaxLengthPassed && 
                           checks.HasNumber && 
                           checks.HasSpecialChar && 
                           checks.NoSpaces;

            // -------------------------------------------------------------------------
            // STEP C: SECURE PLANNED AUDITING
            // We write the validation metrics to Application Insights for operations monitoring.
            // SECURITY REQUIREMENT: We NEVER log the actual password string itself. This prevents
            // plain-text credentials from leaking into security monitoring logs.
            // -------------------------------------------------------------------------
            _logger.LogInformation("Validation completed. Overall Valid: {IsValid}. Failures - Length: {LengthFail}, Num: {NumFail}, Special: {SpecialFail}, Spaces: {SpaceFail}",
                isValid,
                (!checks.MinLengthPassed || !checks.MaxLengthPassed), // Records if a length violation occurred
                !checks.HasNumber,                                    // Records if a number check failed
                !checks.HasSpecialChar,                               // Records if a special character check failed
                !checks.NoSpaces);                                    // Records if a space violation occurred

            // -------------------------------------------------------------------------
            // STEP D: PACKAGE AND RETURN THE RESPONSE
            // We pack our structured response object and return it as an HTTP 200 OK.
            // -------------------------------------------------------------------------
            var response = new ValidationResponse
            {
                IsValid = isValid,
                Checks = checks
            };

            // Returns HTTP 200 OK containing our structured JSON results back to the browser
            return new OkObjectResult(response);
        }
    }
}