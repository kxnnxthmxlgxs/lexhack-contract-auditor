import React, { useState, useRef } from 'react';
import { FileText, CheckCircle, AlertTriangle, UploadCloud, XCircle } from 'lucide-react';

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
  
  // Reference to hide the default browser file input
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0]);
      setError(null);
      setAuditData(null);
    }
  };

  const handleClearFile = (e: React.MouseEvent) => {
    e.stopPropagation();
    setFile(null);
    setAuditData(null);
    setError(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);
    setError(null);
    setAuditData(null);

    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await fetch("https://lexhack-api.onrender.com/api/audit/upload", {
        method: "POST",
        body: formData,
      });

      if (!res.ok) {
          throw new Error("AI Analysis Timeout or Server Error. The Gemini model may be experiencing high traffic. Please try again.");
      }

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
        setError("An unknown error occurred while connecting to the AI engine.");
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
          <p className="text-gray-500 mt-2 text-lg">LexHack 2026 - AI-Powered Deviation Detection</p>
        </header>

        {/* Upload Section */}
        <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-200 text-center max-w-2xl mx-auto">
          
          {/* Custom Dropzone UI */}
          <div 
            onClick={() => !loading && fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-xl p-8 transition-colors ${loading ? 'border-gray-200 bg-gray-50 cursor-not-allowed' : 'border-blue-300 bg-blue-50/50 hover:bg-blue-50 hover:border-blue-500 cursor-pointer'}`}
          >
            <input 
              type="file" 
              ref={fileInputRef}
              accept="application/pdf" 
              onChange={handleFileChange} 
              className="hidden" 
            />
            
            {file ? (
              <div className="flex flex-col items-center space-y-2">
                <FileText className="w-12 h-12 text-blue-600" />
                <span className="text-gray-800 font-semibold">{file.name}</span>
                <span className="text-sm text-gray-500">{(file.size / 1024 / 1024).toFixed(2)} MB</span>
                {!loading && (
                    <button 
                      onClick={handleClearFile}
                      className="text-red-500 hover:text-red-700 text-sm mt-2 flex items-center font-medium"
                    >
                      <XCircle className="w-4 h-4 mr-1" /> Remove Document
                    </button>
                )}
              </div>
            ) : (
              <div className="flex flex-col items-center space-y-3">
                <div className="p-3 bg-blue-100 rounded-full">
                  <UploadCloud className="w-8 h-8 text-blue-600" />
                </div>
                <span className="text-gray-700 font-medium text-lg">Click to upload a contract PDF</span>
                <span className="text-sm text-gray-500">Powered by Google Gemini 2.5 Flash</span>
              </div>
            )}
          </div>

          <button 
            onClick={handleUpload} 
            disabled={!file || loading}
            className="mt-6 w-full py-3 bg-blue-600 text-white rounded-lg font-medium text-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all flex items-center justify-center shadow-sm"
          >
            {loading ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Analyzing with Gemini...
              </>
            ) : "Run Playbook Audit"}
          </button>
          
          {error && (
            <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start text-left">
              <AlertTriangle className="w-5 h-5 text-red-600 mr-2 flex-shrink-0 mt-0.5" />
              <p className="text-red-700 text-sm font-medium">{error}</p>
            </div>
          )}
        </div>

        {/* Results Section */}
        {auditData && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 animate-in fade-in duration-500">
            
            {/* Risk Scorecard */}
            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 md:col-span-1 flex flex-col justify-center">
              <h2 className="text-xl font-semibold text-gray-800 mb-6 text-center">Risk Profile</h2>
              <div className={`p-6 rounded-2xl text-center border-2 shadow-sm ${auditData.riskScore === 0 ? 'bg-green-50 border-green-200' : auditData.riskScore > 40 ? 'bg-red-50 border-red-200' : 'bg-yellow-50 border-yellow-200'}`}>
                <div className={`text-7xl font-bold mb-2 tracking-tight ${auditData.riskScore === 0 ? 'text-green-600' : auditData.riskScore > 40 ? 'text-red-600' : 'text-yellow-600'}`}>
                  {auditData.riskScore}
                </div>
                <div className="text-sm font-bold text-gray-500 uppercase tracking-widest">Total Score</div>
              </div>
              <p className={`text-center font-medium mt-6 px-4 py-2 rounded-lg ${auditData.riskScore === 0 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                {auditData.riskScore === 0 ? "✓ Fully Compliant. Safe to execute." : "⚠ Critical deviations detected. Legal review required."}
              </p>
            </div>

            {/* Clause Breakdown */}
            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 md:col-span-2">
              <h2 className="text-xl font-semibold text-gray-800 mb-6 flex items-center">
                <FileText className="w-5 h-5 mr-2 text-gray-500" />
                Rule Evaluation Engine
              </h2>
              <div className="space-y-3">
                {auditData.results.map((result, idx) => (
                  <div key={idx} className="flex flex-col sm:flex-row sm:items-center justify-between p-4 rounded-lg border bg-gray-50 hover:bg-gray-100 transition-colors">
                    <div className="flex items-center space-x-3 mb-2 sm:mb-0">
                      {result.passed ? <CheckCircle className="w-5 h-5 text-green-500 flex-shrink-0" /> : <AlertTriangle className="w-5 h-5 text-red-500 flex-shrink-0" />}
                      <span className="font-semibold text-gray-700">{formatClause(result.clauseType)}</span>
                    </div>
                    <div className="text-sm bg-white px-4 py-2 rounded-md border shadow-sm text-gray-700 font-mono sm:max-w-[50%] truncate text-right">
                      <span className="text-gray-400 mr-2 text-xs uppercase tracking-wider">Found:</span>
                      {result.extractedValue || "Not Found"}
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