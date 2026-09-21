import React, { useState } from 'react';
import { FileText, CheckCircle, AlertTriangle } from 'lucide-react';

// Type definitions matching our C# backend
type AuditResult = {
  clauseType: string;
  extractedValue: string;
  passed: boolean;
};

type AuditRun = {
  fileName: string;
  riskScore: number;
  results: AuditResult[];
};

export default function App() {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [auditData, setAuditData] = useState<AuditRun | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0]);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);
    setError(null);
    setAuditData(null);

    const formData = new FormData();
    formData.append("file", file);

    try {
      // Adjust this port to match whatever your .NET API terminal says it is listening on
      const res = await fetch("/api/audit/upload", {
        method: "POST",
        body: formData,
      });

      if (!res.ok) throw new Error("API Request Failed. Ensure the backend is running.");

      const data = await res.json();
      if (data.success) {
        setAuditData(data.audit);
      } else {
        throw new Error(data.problem || "Extraction failed");
      }
    } catch (err) {
          if (err instanceof Error) {
            setError(err.message);
          } else {
            setError("An unknown error occurred");
          }
        } finally {
          setLoading(false);
        }
  };

  const formatClause = (clause: string) => 
    clause.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');

  return (
    <div className="min-h-screen bg-gray-50 p-8 font-sans">
      <div className="max-w-5xl mx-auto space-y-8">
        
        <header className="text-center">
          <h1 className="text-4xl font-bold text-gray-900">Contract Playbook Auditor</h1>
          <p className="text-gray-500 mt-2">LexHack 2026 - AI-Powered Deviation Detection</p>
        </header>

        {/* Upload Section */}
        <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-200 text-center">
          <div className="flex justify-center mb-4">
            <FileText className="w-12 h-12 text-blue-600" />
          </div>
          <input 
            type="file" 
            accept="application/pdf" 
            onChange={handleFileChange} 
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 mx-auto max-w-xs cursor-pointer" 
          />
          <button 
            onClick={handleUpload} 
            disabled={!file || loading}
            className="mt-6 px-6 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-75 transition-colors flex items-center justify-center mx-auto min-w-[200px]"
          >
            {loading ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Auditing...
              </>
            ) : "Run Playbook Audit"}
          </button>
          {error && <p className="text-red-500 mt-4 text-sm font-medium">{error}</p>}
        </div>

        {/* Results Section */}
        {auditData && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            
            {/* Risk Scorecard */}
            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 md:col-span-1 flex flex-col justify-center">
              <h2 className="text-lg font-semibold text-gray-800 mb-4 text-center">Overall Risk Profile</h2>
              <div className={`p-6 rounded-lg text-center border ${auditData.riskScore === 0 ? 'bg-green-50 border-green-200' : auditData.riskScore > 40 ? 'bg-red-50 border-red-200' : 'bg-yellow-50 border-yellow-200'}`}>
                <div className={`text-6xl font-bold mb-2 ${auditData.riskScore === 0 ? 'text-green-700' : auditData.riskScore > 40 ? 'text-red-700' : 'text-yellow-700'}`}>
                  {auditData.riskScore}
                </div>
                <div className="text-sm font-bold text-gray-600 uppercase tracking-wide">Total Score</div>
              </div>
              <p className="text-sm text-gray-500 mt-4 text-center font-medium">
                {auditData.riskScore === 0 ? "Fully Compliant. Safe to sign." : "Deviations detected. Legal review required."}
              </p>
            </div>

            {/* Clause Breakdown */}
            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 md:col-span-2">
              <h2 className="text-lg font-semibold text-gray-800 mb-4">Rule Evaluation</h2>
              <div className="space-y-3">
                {auditData.results.map((result, idx) => (
                  <div key={idx} className="flex items-center justify-between p-4 rounded-lg border bg-gray-50">
                    <div className="flex items-center space-x-3">
                      {result.passed ? <CheckCircle className="w-5 h-5 text-green-500" /> : <AlertTriangle className="w-5 h-5 text-red-500" />}
                      <span className="font-semibold text-gray-700">{formatClause(result.clauseType)}</span>
                    </div>
                    <div className="text-sm bg-white px-3 py-1 rounded border shadow-sm text-gray-700 font-mono">
                      Found: {result.extractedValue || "N/A"}
                    </div>
                  </div>
                ))}
              </div>
            </div>

          </div>
        )}
      </div>
    </div>
  );
}