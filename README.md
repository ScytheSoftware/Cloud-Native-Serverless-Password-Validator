# 🔒 PassShield: Cloud-Native Serverless Password Validator

### ⚠️ Status: Repository Under Active Modernization (Migration in Progress)
This repository is currently being transformed from a legacy C# local terminal application into an enterprise-grade, serverless cloud microservice.

---

## 📐 Legacy Foundation & The Modern Cloud Vision

### 1. Legacy Codebase (April 2020)
Originally developed as a local, stateful, blocking C# Console Application. It evaluated user console input strings against specific security rules (length, numeric presence, special characters, space checks) using standard console input-output loops (`Console.ReadLine`).

### 2. Modern Serverless Architecture (In Development)
To transition this project into a production-ready application, we are decoupling the legacy logic and shifting to an **event-driven, stateless cloud-native web service**:

```text
  [ Front-End Interface ]               [ Serverless Backend API ]
   Azure Static Web Apps  ───(HTTPS)───►  Azure Function (HTTP Trigger)
   (HTML5, CSS3, Vanilla JS)              C# (.NET 8 Isolated Worker)
             │                                      │
             ▼                                      ▼
   Dynamic UI Checkmarks                   Application Insights
   (Real-time visual validation)           (Secure telemetry tracking)
                                           *Zero raw password logging (PII Protection)*
