# ⚖️ The Contract Playbook Deviation Auditor

**LexHack 2026 Submission**

An AI-powered legal tech application that automates the review of third-party contracts against internal company playbooks. The system extracts raw text from uploaded PDFs, evaluates the clauses using Google's Gemini AI, and generates a dynamic risk scorecard highlighting critical deviations.

## 🚀 Features
* **Intelligent Clause Extraction:** Leverages the native Google Gemini 2.5 Flash REST API to parse complex legal language and securely extract key data points (e.g., Governing Law, Liability Caps, Indemnification).
* **Automated Playbook Auditing:** Evaluates extracted clauses against a strict SQLite database of corporate rules using a custom C# evaluation engine.
* **Dynamic Risk Scoring:** Automatically assigns risk weights (Warning/Critical) to contract deviations and calculates an overall risk score.
* **Bulletproof Edge-Case Handling:** Gracefully handles missing clauses and null values without crashing the evaluation engine.
* **Cloud-Native Architecture:** Fully containerized backend hosted on Render, communicating seamlessly with a fast React frontend on Vercel.

## 💻 Tech Stack
**Frontend:**
* React + Vite
* Tailwind CSS

**Backend:**
* .NET 8 (ASP.NET Core Minimal APIs)
* Entity Framework Core + SQLite
* UglyToad.PdfPig (PDF text extraction)
* Google Gemini REST API (gemini-2.5-flash)

**Infrastructure & Deployment:**
* **Frontend Hosting:** Vercel
* **Backend Hosting:** Render (Docker environment)
* **Version Control:** GitHub

## 🌐 Live Demo
* **Frontend UI:** [Insert your Vercel URL here]
* **Backend API:** [Insert your Render URL here]

## 🛠️ Local Development Setup

### Prerequisites
* .NET 8 SDK
* Node.js & npm
* A Google Gemini API Key

### Backend Setup
1. Navigate to the backend directory: `cd backend/LexHack.Api`
2. Set your Gemini API key in `appsettings.Development.json` (or as a local environment variable `Gemini__ApiKey`).
3. Run the application: `dotnet run`
*(Note: The SQLite database will auto-generate on startup)*

### Frontend Setup
1. Navigate to the frontend directory: `cd frontend/auditor-ui`
2. Install dependencies: `npm install`
3. Ensure the API endpoint in `App.tsx` points to `http://localhost:5000/api/audit/upload` for local testing.
4. Start the Vite development server: `npm run dev`

## 🧠 Architecture Notes
* **Ephemeral Cloud Execution:** The backend utilizes a custom Dockerfile configured for Render's restricted `app` user permissions. The SQLite database is generated dynamically on startup.
* **Cross-Origin Security:** CORS is explicitly configured to allow secure communication between the Vercel edge network and the Render container.