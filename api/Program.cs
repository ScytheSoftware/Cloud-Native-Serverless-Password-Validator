using System; // Required to read environment variables
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure the web application host
builder.ConfigureFunctionsWebApplication();

// Fetch the Application Insights connection string from system environment variables
var appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

// DYNAMIC TELEMETRY BINDING:
// Only load the Azure Monitor Exporter if a valid connection string is actually provided.
// This prevents local development crashes while allowing automated telemetry in the cloud!
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

// Build and run the serverless host
builder.Build().Run();