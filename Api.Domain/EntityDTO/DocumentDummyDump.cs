using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityDTO
{
	public class DocumentDetail
	{
		public string DocumentName { get; set; }
		public string ReferenceId { get; set; }
		public DocumentDetail(string name) => DocumentName = name;
	}

	public class DocumentCategory
	{
		public string CategoryCode { get; set; }
		public string CategoryName { get; set; }
		public List<DocumentDetail> Documents { get; set; } = new List<DocumentDetail>();
		public List<DocumentCategory> SubCategories { get; set; } = new List<DocumentCategory>();
		public DocumentCategory(string code, string name)
		{
			CategoryCode = code;
			CategoryName = name;
		}
	}

	public class CategoryDataInitializer
	{
		/**
		 * Creates and initializes the full hierarchical list of categories and documents.
		 */
		public List<DocumentCategory> InitializeFullCategoryList()
		{
			// --- 1. Master List to hold all top-level categories ---
			var topLevelCategories = new List<DocumentCategory>();

			// --- 2. Create All Categories and Define Hierarchy ---

			// (0) PERSONAL DOCUMENTS
			var cat0 = new DocumentCategory("(0)", "PERSONAL DOCUMENTS");

			// (1) BASIC GCG DOCUMENTS
			var cat1 = new DocumentCategory("(1)", "BASIC GCG DOCUMENTS");
			var cat1_1 = new DocumentCategory("(1.1)", "LEGAL DOCUMENTS");
			var cat1_2 = new DocumentCategory("(1.2)", "FINANCIAL DOCUMENTS");
			var cat1_3 = new DocumentCategory("(1.3)", "ASSET");
			var cat1_4 = new DocumentCategory("(1.4)", "LIABILITIES DOCUMENTS");
			var cat1_5 = new DocumentCategory("(1.5)", "LICENSES DOCUMENTS");
			var cat1_6 = new DocumentCategory("(1.6)", "PUBLIC COMMITMENT DOCUMENTS");
			var cat1_7 = new DocumentCategory("(1.7)", "CERTIFICATION / REPORTS");
			cat1.SubCategories.AddRange(new[] { cat1_1, cat1_2, cat1_3, cat1_4, cat1_5, cat1_6, cat1_7 });

			// (2) CORPORATE ASPECTS
			var cat2 = new DocumentCategory("(2)", "CORPORATE ASPECTS");
			var cat2_1 = new DocumentCategory("(2.1)", "SHAREHOLDING");
			var cat2_2 = new DocumentCategory("(2.2)", "MINUTES OF BOD MEETINGS");
			var cat2_3 = new DocumentCategory("(2.3)", "MINUTES OF BOC MEETINGS");
			var cat2_4 = new DocumentCategory("(2.4)", "POWER OF ATTORNEY");
			var cat2_5 = new DocumentCategory("(2.5)", "ARTICLE OF ASSOCIATION");
			var cat2_6 = new DocumentCategory("(2.6)", "IMPORTANT CORPORATE ACTIONS");
			var cat2_7 = new DocumentCategory("(2.7)", "OTHERS");
			cat2.SubCategories.AddRange(new[] { cat2_1, cat2_2, cat2_3, cat2_4, cat2_5, cat2_6, cat2_7 });

			// (2.1) Sub-Categories
			var cat2_1_1 = new DocumentCategory("(2.1.1)", "INDEPENDENTS/PUBLIC");
			var cat2_1_2 = new DocumentCategory("(2.1.2)", "MAJOR SHARES");
			cat2_1.SubCategories.AddRange(new[] { cat2_1_1, cat2_1_2 });

			// (2.6) Sub-Categories
			var cat2_6_1 = new DocumentCategory("(2.6.1)", "MATERIAL TRANSACTIONS");
			var cat2_6_2 = new DocumentCategory("(2.6.2)", "AFFILIATED TRANSACTIONS");
			var cat2_6_3 = new DocumentCategory("(2.6.3)", "CONFLICT OF INTEREST TRANSACTION");
			var cat2_6_4 = new DocumentCategory("(2.6.4)", "OTHERS TRANSACTION");
			cat2_6.SubCategories.AddRange(new[] { cat2_6_1, cat2_6_2, cat2_6_3, cat2_6_4 });

			// (3) CONTRACTS
			var cat3 = new DocumentCategory("(3)", "CONTRACTS");
			var cat3_1 = new DocumentCategory("(3.1)", "FINANCE");
			var cat3_2 = new DocumentCategory("(3.2)", "PURCHASING");
			var cat3_3 = new DocumentCategory("(3.3)", "MARKETING");
			var cat3_4 = new DocumentCategory("(3.4)", "HUMAN RESOURCES");
			var cat3_5 = new DocumentCategory("(3.5)", "LAND MANAGEMENT");
			cat3.SubCategories.AddRange(new[] { cat3_1, cat3_2, cat3_3, cat3_4, cat3_5 });

			// (3.4) Sub-Categories
			var cat3_4_1 = new DocumentCategory("(3.4.1)", "PERMANENT EMPLOYEE");
			var cat3_4_2 = new DocumentCategory("(3.4.2)", "CONTRACT EMPLOYEE");
			cat3_4.SubCategories.AddRange(new[] { cat3_4_1, cat3_4_2 });

			// (4) EMPLOYMENT
			var cat4 = new DocumentCategory("(4)", "EMPLOYMENT");

			// (5) DISPUTES
			var cat5 = new DocumentCategory("(5)", "DISPUTES");
			var cat5_1 = new DocumentCategory("(5.1)", "ON GOING DISPUTES");
			var cat5_2 = new DocumentCategory("(5.2)", "POTENTIAL DISPUTES");
			cat5.SubCategories.AddRange(new[] { cat5_1, cat5_2 });

			// (6) REGULATORY ISSUES
			var cat6 = new DocumentCategory("(6)", "REGULATORY ISSUES");

			// (7) HUMAN RESOURCES (HR)
			var cat7 = new DocumentCategory("(7)", "HUMAN RESOURCES (HR)");

			topLevelCategories.AddRange(new[] { cat0, cat1, cat2, cat3, cat4, cat5, cat6, cat7 });


			// --- 3. Populate Documents (Data from the Table) ---

			// (0) PERSONAL DOCUMENTS
			cat0.Documents.AddRange(new[]
			{
			new DocumentDetail("Employee Personal Data Protection Policy"), new DocumentDetail("Employee Personnel File Checklist and Index"),
			new DocumentDetail("Confidentiality Agreement for HR Staff Handling Personal Data"), new DocumentDetail("Personal Contact and Emergency Information Form"),
			new DocumentDetail("Annual Conflict of Interest Disclosure Form"), new DocumentDetail("Employee Signature and Verification Card"),
			new DocumentDetail("Personal Training and Certification Records"), new DocumentDetail("Employee Health and Medical Records Policy"),
			new DocumentDetail("Background Check Consent and Results Document"), new DocumentDetail("Personal Data Breach Response Protocol"),
			new DocumentDetail("Internal Memo on Personal Data Retention Schedule"), new DocumentDetail("Employee Next-of-Kin Nomination Form"),
			new DocumentDetail("Personal Vehicle Use and Reimbursement Form"), new DocumentDetail("Employee Exit Survey Raw Data (Anonymized)"),
			new DocumentDetail("Policy on Use of Personal Devices at Work")
		});

			// (1) BASIC GCG DOCUMENTS
			cat1.Documents.AddRange(new[]
			{
			new DocumentDetail("Good Corporate Governance (GCG) Policy Framework"), new DocumentDetail("Board of Directors (BOD) Charter"),
			new DocumentDetail("Board of Commissioners (BOC) Charter"), new DocumentDetail("Code of Ethics and Business Conduct"),
			new DocumentDetail("Internal Audit Charter and Manual"), new DocumentDetail("Whistleblowing Policy and Procedure"),
			new DocumentDetail("Risk Management Policy and Guidelines"), new DocumentDetail("Corporate Social Responsibility (CSR) Framework"),
			new DocumentDetail("Investor Relations Policy"), new DocumentDetail("Related Party Transaction Policy"),
			new DocumentDetail("GCG Self-Assessment Report 20XX"), new DocumentDetail("Anti-Bribery and Corruption (ABC) Policy"),
			new DocumentDetail("Conflict of Interest Policy"), new DocumentDetail("Corporate Secretary Standard Operating Procedures (SOP)"),
			new DocumentDetail("Annual GCG Compliance Checklist")
		});

			// (1.1) LEGAL DOCUMENTS
			cat1_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Corporate Legal Filings Master Index"), new DocumentDetail("Litigation and Claim Status Summary"),
			new DocumentDetail("Power of Attorney Register and Review Log"), new DocumentDetail("External Legal Counsel Fee Structure Agreement"),
			new DocumentDetail("Standard Operating Procedure (SOP) for Document Execution"), new DocumentDetail("Legal Opinion Request Form and Log"),
			new DocumentDetail("Legal Risk Assessment Framework Document"), new DocumentDetail("Regulatory Warning and Penalty Response Plan"),
			new DocumentDetail("Legal Department Annual Report 20XX"), new DocumentDetail("Legal Hold Notice Template"),
			new DocumentDetail("Statutory Books and Records Management Guide"), new DocumentDetail("Trademarks and Patents Registration Documents"),
			new DocumentDetail("Legal Document Retention and Destruction Policy"), new DocumentDetail("Internal Legal Memo on New Legislation"),
			new DocumentDetail("Legal Document Template Library Index")
		});

			// (1.2) FINANCIAL DOCUMENTS
			cat1_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Annual Financial Statements (Audited) 20XX"), new DocumentDetail("Budgeting and Forecasting Policy Manual"),
			new DocumentDetail("Fixed Asset Register and Depreciation Schedule"), new DocumentDetail("Internal Controls over Financial Reporting (ICFR) Document"),
			new DocumentDetail("Treasury and Cash Management Policy"), new DocumentDetail("Tax Compliance and Filing Documentation"),
			new DocumentDetail("Interim Financial Report Template"), new DocumentDetail("Accounts Payable and Receivable Procedure"),
			new DocumentDetail("Financial Delegation of Authority Matrix"), new DocumentDetail("Capital Expenditure (CAPEX) Approval Process"),
			new DocumentDetail("Financial Data Security and Backup Policy"), new DocumentDetail("Debt and Financing Agreement Summaries"),
			new DocumentDetail("Investment Portfolio Management Report"), new DocumentDetail("Fraud Detection and Prevention Policy"),
			new DocumentDetail("Quarterly Financial Performance Analysis")
		});

			// (1.3) ASSET
			cat1_3.Documents.AddRange(new[]
			{
			new DocumentDetail("Fixed Asset Management Policy"), new DocumentDetail("Asset Tagging and Inventory Procedure"),
			new DocumentDetail("Asset Disposal and Sale Protocol"), new DocumentDetail("Detailed Asset Valuation Report"),
			new DocumentDetail("IT Hardware and Software Asset Register"), new DocumentDetail("Intellectual Property (IP) Asset Protection Strategy"),
			new DocumentDetail("Asset Insurance Coverage Summary Document"), new DocumentDetail("Maintenance Schedule for Major Assets"),
			new DocumentDetail("Asset Impairment Testing Procedure"), new DocumentDetail("Vehicle Fleet Management Policy"),
			new DocumentDetail("Asset Handover and Transfer Form"), new DocumentDetail("Asset Acquisition Request and Approval Workflow"),
			new DocumentDetail("Inventory Count and Reconciliation Procedure"), new DocumentDetail("Digital Asset Management (DAM) Guidelines"),
			new DocumentDetail("Report on Utilization of Key Assets")
		});

			// (1.4) LIABILITIES DOCUMENTS
			cat1_4.Documents.AddRange(new[]
			{
			new DocumentDetail("Liabilities and Debt Register Master Log"), new DocumentDetail("Loan Agreement and Covenant Compliance Checklist"),
			new DocumentDetail("Provision and Contingent Liability Assessment Report"), new DocumentDetail("Accounts Payable Aging Report"),
			new DocumentDetail("Tax Liability Calculation and Reconciliation File"), new DocumentDetail("Employee Benefit Obligation (EBO) Valuation Report"),
			new DocumentDetail("Guarantee and Suretyship Documentation Summary"), new DocumentDetail("Policy on Recording and Reporting Liabilities"),
			new DocumentDetail("Credit Facility Agreement Terms Summary"), new DocumentDetail("Warranties and Returns Provision Calculation"),
			new DocumentDetail("Off-Balance Sheet Liabilities Disclosure Document"), new DocumentDetail("Legal Settlement and Obligation Records"),
			new DocumentDetail("Intercompany Loan Agreement Templates"), new DocumentDetail("Bond Issuance and Prospectus Documentation"),
			new DocumentDetail("Quarterly Liability Status Report")
		});

			// (1.5) LICENSES DOCUMENTS
			cat1_5.Documents.AddRange(new[]
			{
			new DocumentDetail("Master License and Permit Register and Renewal Tracker"), new DocumentDetail("Business Operating License Compliance Report"),
			new DocumentDetail("Software Licensing and Usage Policy"), new DocumentDetail("Intellectual Property (IP) License Agreement Templates"),
			new DocumentDetail("Regulatory Permit Application Procedure"), new DocumentDetail("Environmental License Monitoring Checklist"),
			new DocumentDetail("Industry-Specific Certification Renewal Guide"), new DocumentDetail("Government License Fee Payment Schedule"),
			new DocumentDetail("License Audit Compliance Documentation"), new DocumentDetail("Internal Memo on New Licensing Requirements"),
			new DocumentDetail("Policy on Open Source Software Use"), new DocumentDetail("Export/Import License Documentation"),
			new DocumentDetail("Summary of Key License Restrictions"), new DocumentDetail("Communication Log with Licensing Authorities"),
			new DocumentDetail("License Non-Compliance Action Plan")
		});

			// (1.6) PUBLIC COMMITMENT DOCUMENTS
			cat1_6.Documents.AddRange(new[]
			{
			new DocumentDetail("Corporate Social Responsibility (CSR) Annual Report"), new DocumentDetail("Stakeholder Engagement Plan and Communication Protocol"),
			new DocumentDetail("Public Disclosure and Transparency Policy"), new DocumentDetail("Environmental, Social, and Governance (ESG) Reporting Framework"),
			new DocumentDetail("Sustainability Goals and Progress Report 20XX"), new DocumentDetail("Donation and Sponsorship Policy and Approval Form"),
			new DocumentDetail("Code of Conduct for Suppliers and Partners"), new DocumentDetail("Internal Memo on Public Commitment Compliance"),
			new DocumentDetail("Community Grievance Mechanism and Procedure"), new DocumentDetail("Public Commitment Tracking and Verification Document"),
			new DocumentDetail("Press Release Approval and Issuance Protocol"), new DocumentDetail("Customer Service Charter and Commitments"),
			new DocumentDetail("Ethical Sourcing Policy Statement"), new DocumentDetail("Anti-Corruption and Whistleblower Public Statement"),
			new DocumentDetail("Annual Public Commitment Compliance Audit")
		});

			// (1.7) CERTIFICATION / REPORTS
			cat1_7.Documents.AddRange(new[]
			{
			new DocumentDetail("ISO Certification Maintenance Manual"), new DocumentDetail("Annual External Audit Report (Financial)"),
			new DocumentDetail("Internal Control Effectiveness Report"), new DocumentDetail("Management System Review Minutes (e.g., Quality, Safety)"),
			new DocumentDetail("Sustainability Certification Documentation"), new DocumentDetail("Data Security Penetration Test Report Summary"),
			new DocumentDetail("Regulatory Compliance Certification Log"), new DocumentDetail("Quality Assurance and Testing Report Template"),
			new DocumentDetail("Third-Party Certification Audit Action Plan"), new DocumentDetail("Internal Quality Control Checklist"),
			new DocumentDetail("Energy Consumption and Efficiency Report"), new DocumentDetail("Environmental Permit Monitoring Reports"),
			new DocumentDetail("Product Safety and Compliance Certificates"), new DocumentDetail("Certification Expiry and Renewal Calendar"),
			new DocumentDetail("Accreditation and Recognition Documentation")
		});

			// (2) CORPORATE ASPECTS
			cat2.Documents.AddRange(new[]
			{
			new DocumentDetail("Corporate Legal Structure and Shareholding Diagram"), new DocumentDetail("Memorandum and Articles of Association (M&A)"),
			new DocumentDetail("Annual General Meeting of Shareholders (AGMS) Procedure"), new DocumentDetail("Corporate Seal and Statutory Register Log"),
			new DocumentDetail("Company Registration and Licensing Documentation"), new DocumentDetail("Intercompany Agreement Templates"),
			new DocumentDetail("Corporate Governance Reporting Standard"), new DocumentDetail("Corporate Compliance Calendar"),
			new DocumentDetail("Non-Disclosure Agreement (NDA) Master Template"), new DocumentDetail("Intellectual Property (IP) Registration Portfolio"),
			new DocumentDetail("Corporate Record Retention Policy"), new DocumentDetail("Subsidiary Management and Oversight Protocol"),
			new DocumentDetail("Company Name and Trademark Usage Guidelines"), new DocumentDetail("Shareholder Communications Policy"),
			new DocumentDetail("Corporate Insurance Policies Summary")
		});

			// (2.1) SHAREHOLDING
			cat2_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Shareholder Register and Transaction Log"), new DocumentDetail("Policy on Share Transfer and Issuance"),
			new DocumentDetail("Major Shareholder Communication Protocol"), new DocumentDetail("Dividend Policy and Payment Procedure"),
			new DocumentDetail("Shareholders Agreement Master Document"), new DocumentDetail("Annual Shareholder Structure Report"),
			new DocumentDetail("Warrant and Option Plan Documentation"), new DocumentDetail("Share Capital Increase/Decrease Resolution"),
			new DocumentDetail("Shareholder Meeting Attendance and Proxy Forms"), new DocumentDetail("Securities Trading Policy for Insiders"),
			new DocumentDetail("Shareholder Data Privacy and Security Policy"), new DocumentDetail("Policy on Share Buyback Programs"),
			new DocumentDetail("Share Allotment and Issuance Certificates"), new DocumentDetail("Shareholder Complaint Resolution Procedure"),
			new DocumentDetail("Historical Shareholding Changes Summary")
		});

			// (2.1.1) INDEPENDENTS/PUBLIC
			cat2_1_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Public Shareholder Communication Strategy"), new DocumentDetail("Policy on Investor Relations and Market Disclosure"),
			new DocumentDetail("Independent Shareholder Meeting Minutes"), new DocumentDetail("Log of Communications with Public Investors"),
			new DocumentDetail("Independent Shareholder Feedback Summary Report"), new DocumentDetail("Template for Public Shareholder Queries Response"),
			new DocumentDetail("Independent Shareholder Proxy Voting Analysis"), new DocumentDetail("Policy on Managing Insider Trading Risks"),
			new DocumentDetail("Public Float Calculation and Monitoring Report"), new DocumentDetail("Internal Memo on Independent Shareholder Rights"),
			new DocumentDetail("Securities Exchange Filing Compliance Checklist"), new DocumentDetail("Policy on Engaging with Institutional Investors"),
			new DocumentDetail("Public Roadshow Presentation Materials"), new DocumentDetail("Report on Independent Shareholder Meeting Attendance"),
			new DocumentDetail("Policy on Fair Treatment of All Shareholders")
		});

			// (2.1.2) MAJOR SHARES
			cat2_1_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Major Shareholder Communication Protocol"), new DocumentDetail("Internal Policy on Monitoring Major Share Ownership Changes"),
			new DocumentDetail("Regulatory Filing Compliance for Major Share Acquisitions/Disposals"), new DocumentDetail("Confidentiality Agreement with Major Shareholders"),
			new DocumentDetail("Summary of Major Shareholder Rights and Obligations"), new DocumentDetail("Board Minutes Discussing Major Shareholder Issues"),
			new DocumentDetail("Major Shareholder Profile and Historical Data"), new DocumentDetail("Policy on Dealing with Aggressive Activist Shareholders"),
			new DocumentDetail("Proxy Voting Recommendations for Major Shareholders"), new DocumentDetail("Legal Opinion on Major Shareholder Influence"),
			new DocumentDetail("Major Shareholder Lock-up Agreement Document"), new DocumentDetail("Internal Memo on Shareholder Activism Preparedness"),
			new DocumentDetail("Major Shareholder Annual Engagement Report"), new DocumentDetail("Share Purchase/Sale Agreement with Major Shareholder"),
			new DocumentDetail("Policy on Fair and Equal Treatment of Major Shareholders")
		});

			// (2.2) MINUTES OF BOD MEETINGS
			cat2_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Board of Directors (BOD) Meeting Minutes Archival Index"), new DocumentDetail("BOD Meeting Agenda Template and Protocol"),
			new DocumentDetail("BOD Resolution and Decision Register"), new DocumentDetail("BOD Information Pack Preparation Guide"),
			new DocumentDetail("Action Item Tracker for BOD Meetings"), new DocumentDetail("BOD Meeting Attendance Log and Quorum Verification"),
			new DocumentDetail("BOD Committee Meeting Minutes Templates"), new DocumentDetail("BOD Meeting Confidentiality Agreement"),
			new DocumentDetail("Internal Memo on BOD Decision Implementation"), new DocumentDetail("BOD Director Performance Review Minutes"),
			new DocumentDetail("BOD Annual Calendar of Meetings"), new DocumentDetail("BOD Meeting Pre-Read Material Checklist"),
			new DocumentDetail("BOD Minutes Review and Approval Process"), new DocumentDetail("BOD Meeting Technology and Recording Policy"),
			new DocumentDetail("Historical BOD Minutes Summary and Analysis")
		});

			// (2.3) MINUTES OF BOC MEETINGS
			cat2_3.Documents.AddRange(new[]
			{
			new DocumentDetail("Board of Commissioners (BOC) Meeting Minutes Index"), new DocumentDetail("BOC Meeting Agenda Template and Protocol"),
			new DocumentDetail("BOC Resolution and Decision Register"), new DocumentDetail("BOC Information Pack Preparation Guide"),
			new DocumentDetail("Action Item Tracker for BOC Meetings"), new DocumentDetail("BOC Meeting Attendance Log and Quorum Verification"),
			new DocumentDetail("BOC Committee Meeting Minutes Templates"), new DocumentDetail("BOC Meeting Confidentiality Agreement"),
			new DocumentDetail("Internal Memo on BOC Oversight Findings"), new DocumentDetail("BOC Commissioner Performance Review Minutes"),
			new DocumentDetail("BOC Annual Calendar of Meetings"), new DocumentDetail("BOC Meeting Pre-Read Material Checklist"),
			new DocumentDetail("BOC Minutes Review and Approval Process"), new DocumentDetail("BOC Meeting Technology and Recording Policy"),
			new DocumentDetail("Historical BOC Minutes Summary and Analysis")
		});

			// (2.4) POWER OF ATTORNEY
			cat2_4.Documents.AddRange(new[]
			{
			new DocumentDetail("Master Power of Attorney (POA) Register and Expiry Tracker"), new DocumentDetail("POA Issuance and Revocation Procedure Manual"),
			new DocumentDetail("Standard POA Template for General Purposes"), new DocumentDetail("Internal Memo on Delegation of Authority via POA"),
			new DocumentDetail("Specific POA for Litigation Template"), new DocumentDetail("POA Document Archival and Security Protocol"),
			new DocumentDetail("POA Verification and Usage Checklist"), new DocumentDetail("Legal Opinion on POA Validity and Scope"),
			new DocumentDetail("POA Granting Authority Matrix"), new DocumentDetail("POA Revocation Notice and Acknowledgment Form"),
			new DocumentDetail("Summary of POA Holders and Their Authority"), new DocumentDetail("POA Training Materials for Staff"),
			new DocumentDetail("Historical POA Usage Log"), new DocumentDetail("Regulatory Requirements for POA Document"),
			new DocumentDetail("POA Confidentiality Agreement")
		});

			// (2.5) ARTICLE OF ASSOCIATION
			cat2_5.Documents.AddRange(new[]
			{
			new DocumentDetail("Current and Historical Articles of Association Master File"), new DocumentDetail("Internal Memo on Amendments to Articles"),
			new DocumentDetail("Statutory Filing Documentation for Articles Changes"), new DocumentDetail("Articles of Association Key Provisions Summary"),
			new DocumentDetail("Checklist for Consistency with Corporate Law"), new DocumentDetail("Policy on Safe Custody of Original Articles"),
			new DocumentDetail("Articles of Association Translation Verification Report"), new DocumentDetail("Internal Review of Articles Compliance"),
			new DocumentDetail("Shareholder Resolution Approving Articles Amendment"), new DocumentDetail("Extract of Articles for Third Parties"),
			new DocumentDetail("Annotated Version of Current Articles"), new DocumentDetail("Articles of Association Comparison Document (Old vs. New)"),
			new DocumentDetail("Bylaws and Operating Agreement Documentation"), new DocumentDetail("Legal Opinion on Articles Validity"),
			new DocumentDetail("Process for Ratifying Articles Amendments")
		});

			// (2.6) IMPORTANT CORPORATE ACTIONS
			cat2_6.Documents.AddRange(new[]
			{
			new DocumentDetail("Corporate Action Master Log and Timeline"), new DocumentDetail("Procedure for Major Transaction Approval"),
			new DocumentDetail("Public Announcement and Disclosure Protocol for Corporate Actions"), new DocumentDetail("Due Diligence Checklist for Corporate Actions"),
			new DocumentDetail("Board Resolution Approving Corporate Action"), new DocumentDetail("Shareholder Circular for Extraordinary Action"),
			new DocumentDetail("Internal Memo on Key Corporate Action Rationale"), new DocumentDetail("Corporate Action Press Release Template"),
			new DocumentDetail("Regulatory Filing for Corporate Actions"), new DocumentDetail("Post-Action Integration and Compliance Report"),
			new DocumentDetail("Valuation Report for Corporate Actions"), new DocumentDetail("Legal Opinion on Corporate Action Legality"),
			new DocumentDetail("Corporate Action Risk Assessment Document"), new DocumentDetail("Timeline for Extraordinary General Meeting (EGM)"),
			new DocumentDetail("Implementation Plan for Corporate Restructuring")
		});

			// (2.6.1) MATERIAL TRANSACTIONS
			cat2_6_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Material Transaction Policy and Definition Manual"), new DocumentDetail("Board Resolution for Material Transaction Approval"),
			new DocumentDetail("Detailed Financial Projections for Material Transaction"), new DocumentDetail("Material Transaction Due Diligence Report"),
			new DocumentDetail("Regulatory Approval Filings for Material Transaction"), new DocumentDetail("Public Disclosure and Press Release for Material Transaction"),
			new DocumentDetail("Fairness Opinion for Material Transaction"), new DocumentDetail("Material Transaction Integration/Separation Plan"),
			new DocumentDetail("Legal Opinion on Material Transaction Structure"), new DocumentDetail("Material Transaction Risk Mitigation Strategy"),
			new DocumentDetail("Shareholder Circular for Material Transaction"), new DocumentDetail("Material Transaction Valuation Report"),
			new DocumentDetail("Definitive Agreement for Material Transaction (e.g., SPA)"), new DocumentDetail("Negotiation Minutes for Material Transaction"),
			new DocumentDetail("Post-Closing Compliance Certificate for Material Transaction")
		});

			// (2.6.2) AFFILIATED TRANSACTIONS
			cat2_6_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Affiliated Transaction Policy and Procedure"), new DocumentDetail("Board/Committee Approval Minutes for Affiliated Transaction"),
			new DocumentDetail("Affiliated Transaction Disclosure and Reporting Form"), new DocumentDetail("Internal Memo on Business Rationale for Affiliated Transaction"),
			new DocumentDetail("Affiliated Transaction Arm's Length Pricing Justification"), new DocumentDetail("Regulatory Filings for Affiliated Transaction"),
			new DocumentDetail("Summary of Intercompany Agreements"), new DocumentDetail("Legal Opinion on Affiliated Transaction Compliance"),
			new DocumentDetail("Affiliated Transaction Monitoring Report"), new DocumentDetail("Annual Summary of All Affiliated Transactions"),
			new DocumentDetail("Policy on Preventing Unfair Affiliated Transactions"), new DocumentDetail("Documentation of Intragroup Services Fees"),
			new DocumentDetail("Tax Implications Report for Affiliated Transactions"), new DocumentDetail("Affiliated Transaction Risk Assessment"),
			new DocumentDetail("Historical Log of Affiliated Transaction Reviews")
		});

			// (2.6.3) CONFLICT OF INTEREST TRANSACTION
			cat2_6_3.Documents.AddRange(new[]
			{
			new DocumentDetail("Conflict of Interest (COI) Transaction Policy Manual"), new DocumentDetail("COI Disclosure and Approval Form"),
			new DocumentDetail("Board/Committee Review Minutes for COI Transaction"), new DocumentDetail("Annual COI Declaration Form for Executives"),
			new DocumentDetail("COI Transaction Fairness Opinion Report"), new DocumentDetail("Internal Memo on Rationale for COI Transaction"),
			new DocumentDetail("Regulatory Filing for COI Transaction"), new DocumentDetail("COI Transaction Negotiation Documentation"),
			new DocumentDetail("COI Transaction Monitoring and Compliance Report"), new DocumentDetail("Summary of Policies Preventing Undisclosed COI"),
			new DocumentDetail("COI Transaction Independent Valuation Document"), new DocumentDetail("COI Transaction Public Disclosure Text"),
			new DocumentDetail("Legal Opinion on COI Transaction Compliance"), new DocumentDetail("COI Transaction Risk Mitigation Strategy"),
			new DocumentDetail("Historical Log of Approved COI Transactions")
		});

			// (2.6.4) OTHERS TRANSACTION
			cat2_6_4.Documents.AddRange(new[]
			{
			new DocumentDetail("Policy on Minor Transaction Approval"), new DocumentDetail("Internal Memo on Non-Material Transaction Reporting"),
			new DocumentDetail("Log of Board Approvals for Non-Material Transactions"), new DocumentDetail("Procedure for Routine Contract Review"),
			new DocumentDetail("Transaction Compliance Checklist (General)"), new DocumentDetail("Documentation for Capital Leases and Financing Arrangements"),
			new DocumentDetail("Standard Operating Procedure for Small Asset Purchases"), new DocumentDetail("Inter-Departmental Service Agreement Documentation"),
			new DocumentDetail("General Business Agreement Template Library"), new DocumentDetail("Log of Waivers and Exceptions to Standard Procedure"),
			new DocumentDetail("Policy on Inter-Company Service Charges"), new DocumentDetail("External Consultation Report for Minor Projects"),
			new DocumentDetail("Record of Small-Scale Investment Decisions"), new DocumentDetail("Internal Audit of General Transaction Compliance"),
			new DocumentDetail("Non-Material Transaction Risk Assessment")
		});

			// (2.7) OTHERS
			cat2_7.Documents.AddRange(new[]
			{
			new DocumentDetail("General Corporate Correspondence Filing Index"), new DocumentDetail("Miscellaneous Legal Opinion Repository"),
			new DocumentDetail("Internal Task Force/Committee Charter (Non-BOD/BOC)"), new DocumentDetail("Uncategorized Corporate Records Master Log"),
			new DocumentDetail("Inter-Departmental Service Level Agreements (SLAs)"), new DocumentDetail("Policy on General Information Disclosure"),
			new DocumentDetail("Archived Project Documentation Index"), new DocumentDetail("General Meeting Minutes (Non-AGMS/EGM)"),
			new DocumentDetail("External Stakeholder Communication Log (General)"), new DocumentDetail("Internal Memo on Corporate Policy Clarifications"),
			new DocumentDetail("Administrative Procedure Manual (General)"), new DocumentDetail("General Regulatory Inquiries Log"),
			new DocumentDetail("Unofficial Corporate History Document"), new DocumentDetail("Standard Template for Internal Memos"),
			new DocumentDetail("Corporate Gift and Entertainment Register")
		});

			// (3) CONTRACTS
			cat3.Documents.AddRange(new[]
			{
			new DocumentDetail("Contract Lifecycle Management (CLM) Policy"), new DocumentDetail("Standard Contract Clauses Library"),
			new DocumentDetail("Contract Drafting and Review Checklist"), new DocumentDetail("Contract Approval Workflow Document"),
			new DocumentDetail("Master Service Agreement (MSA) Template"), new DocumentDetail("Contract Deviation and Exception Log"),
			new DocumentDetail("Contract Termination and Expiration Procedure"), new DocumentDetail("Confidentiality and Data Protection Clauses Standard"),
			new DocumentDetail("Contract Filing and Archiving Protocol"), new DocumentDetail("Risk Assessment for High-Value Contracts"),
			new DocumentDetail("Contract Performance Monitoring Report Template"), new DocumentDetail("Indemnification and Limitation of Liability Guide"),
			new DocumentDetail("Standard Vendor/Client Agreement Template"), new DocumentDetail("Contract Renewal Decision Matrix"),
			new DocumentDetail("Contract Redlining and Negotiation Guidelines")
		});

			// (3.1) FINANCE
			cat3_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Financial Accounting Standard Operating Procedures (SOP)"), new DocumentDetail("Monthly Financial Closing Checklist"),
			new DocumentDetail("Corporate Budget Allocation and Monitoring Report"), new DocumentDetail("Investment Policy and Guidelines"),
			new DocumentDetail("Working Capital Management Strategy"), new DocumentDetail("Financial Risk Assessment and Hedging Policy"),
			new DocumentDetail("Debt Covenant Compliance Certificate"), new DocumentDetail("Cash Flow Forecast and Analysis Report"),
			new DocumentDetail("Accounts Reconciliation Policy and Procedure"), new DocumentDetail("External Audit Engagement Letter and Plan"),
			new DocumentDetail("Fixed Income and Equity Portfolio Summary"), new DocumentDetail("Treasury Operations Manual"),
			new DocumentDetail("Bank Account Management and Reconciliation Policy"), new DocumentDetail("Tax Planning and Compliance Strategy 20XX"),
			new DocumentDetail("Financial Due Diligence Checklist for Acquisitions")
		});

			// (3.2) PURCHASING
			cat3_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Procurement Policy Manual"), new DocumentDetail("Supplier Selection and Vetting Procedures"),
			new DocumentDetail("Purchase Order Request Form Template"), new DocumentDetail("Contract Management Guidelines for Vendors"),
			new DocumentDetail("Vendor Performance Evaluation Report"), new DocumentDetail("Annual Spending and Budget Analysis"),
			new DocumentDetail("Strategic Sourcing Plan 20XX"), new DocumentDetail("Inventory Management Protocol"),
			new DocumentDetail("Capital Expenditure Request Document"), new DocumentDetail("Goods Received Note (GRN) Template"),
			new DocumentDetail("Ethical Sourcing and Sustainability Policy"), new DocumentDetail("Purchase Requisition Flowchart"),
			new DocumentDetail("Approved Vendor List (AVL) Master Document"), new DocumentDetail("Quarterly Purchasing Activity Summary"),
			new DocumentDetail("Risk Assessment for Key Suppliers")
		});

			// (3.3) MARKETING
			cat3_3.Documents.AddRange(new[]
			{
			new DocumentDetail("Annual Marketing Strategy and Budget 20XX"), new DocumentDetail("Brand Guidelines and Style Manual"),
			new DocumentDetail("Market Research and Competitor Analysis Report"), new DocumentDetail("Digital Marketing Campaign Performance Summary"),
			new DocumentDetail("Public Relations Crisis Communication Plan"), new DocumentDetail("Customer Segmentation Analysis Document"),
			new DocumentDetail("Product Launch Go-to-Market Plan"), new DocumentDetail("Social Media Content Calendar Template"),
			new DocumentDetail("Sales and Marketing Alignment Protocol"), new DocumentDetail("Advertising Compliance Checklist"),
			new DocumentDetail("Customer Feedback and Testimonial Collection Process"), new DocumentDetail("Marketing Collateral Inventory List"),
			new DocumentDetail("Return on Investment (ROI) Report for Major Campaigns"), new DocumentDetail("Content Strategy and Editorial Guidelines"),
			new DocumentDetail("Lead Generation and Nurturing Workflow")
		});

			// (3.4) HUMAN RESOURCES
			cat3_4.Documents.AddRange(new[]
			{
			new DocumentDetail("Employee Handbook and Code of Conduct"), new DocumentDetail("HR Policy and Procedure Manual"),
			new DocumentDetail("Annual Recruitment Strategy and Needs Assessment"), new DocumentDetail("Employee Performance Review Template"),
			new DocumentDetail("Compensation and Benefits Structure Document"), new DocumentDetail("Onboarding and Offboarding Process Checklist"),
			new DocumentDetail("Training and Development Program Catalog"), new DocumentDetail("Workplace Health and Safety Guidelines"),
			new DocumentDetail("Internal HR Communication Protocol"), new DocumentDetail("Employee Grievance and Disciplinary Procedure"),
			new DocumentDetail("Job Description Template Master File"), new DocumentDetail("Succession Planning Document"),
			new DocumentDetail("Annual Employee Engagement Survey Report"), new DocumentDetail("Payroll Processing and Compliance Guide"),
			new DocumentDetail("Leave and Attendance Policy")
		});

			// (3.4.1) PERMANENT EMPLOYEE
			cat3_4_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Permanent Employment Contract Standard Template"), new DocumentDetail("Benefits Package Summary for Permanent Staff"),
			new DocumentDetail("Annual Permanent Staff Salary Review Documentation"), new DocumentDetail("Performance Management Plan for Full-Time Roles"),
			new DocumentDetail("Long-Term Career Development Pathways"), new DocumentDetail("Permanent Staff Retention Strategy"),
			new DocumentDetail("Permanent Employee Disciplinary Action Form"), new DocumentDetail("Internal Memo on Permanent Staff Policy Changes"),
			new DocumentDetail("Permanent Employee Data Privacy Consent Form"), new DocumentDetail("Policy on Working Hours for Permanent Staff"),
			new DocumentDetail("Permanent Staff Retirement and Pension Guide"), new DocumentDetail("Permanent Employee Training Needs Analysis"),
			new DocumentDetail("Permanent Staff Remote Work Policy"), new DocumentDetail("Internal Transfer and Promotion Protocol for Permanent Roles"),
			new DocumentDetail("Permanent Employee Exit Interview Template")
		});

			// (3.4.2) CONTRACT EMPLOYEE
			cat3_4_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Fixed-Term Contract Standard Agreement"), new DocumentDetail("Guidelines for Hiring Contract Employees"),
			new DocumentDetail("Contract Employee Onboarding Checklist"), new DocumentDetail("Policy on Contractor vs. Employee Classification"),
			new DocumentDetail("Contractor Performance Review Form"), new DocumentDetail("Temporary Staffing Agency Service Level Agreement (SLA)"),
			new DocumentDetail("Time Tracking and Billing Procedure for Contractors"), new DocumentDetail("Internal Memo on Contract Worker Policy"),
			new DocumentDetail("Contract Employee Non-Disclosure Agreement (NDA) Template"), new DocumentDetail("Contract Renewal and Termination Protocol"),
			new DocumentDetail("Contractor Payment Schedule and Process"), new DocumentDetail("Summary of Legal Rights for Contract Workers"),
			new DocumentDetail("Contract Employee Training Access Policy"), new DocumentDetail("Risk Assessment for Contracted Roles"),
			new DocumentDetail("Contract Employee Exit Clearance Form")
		});

			// (3.5) LAND MANAGEMENT
			cat3_5.Documents.AddRange(new[]
			{
			new DocumentDetail("Corporate Real Estate Asset Register"), new DocumentDetail("Land Acquisition and Due Diligence Checklist"),
			new DocumentDetail("Environmental Impact Assessment (EIA) Guidelines"), new DocumentDetail("Property Maintenance and Repair Schedule"),
			new DocumentDetail("Lease Agreement Standard Template"), new DocumentDetail("Land Use Compliance and Zoning Document"),
			new DocumentDetail("Site Security and Access Protocol"), new DocumentDetail("Land Boundary and Survey Report Files"),
			new DocumentDetail("Property Tax and Insurance Records Summary"), new DocumentDetail("Facilities Management Operations Manual"),
			new DocumentDetail("Deed and Title Verification Process"), new DocumentDetail("Land Valuation and Appraisal Reports Master Log"),
			new DocumentDetail("Property Disposal and Sale Procedure"), new DocumentDetail("Rights-of-Way and Easement Documentation"),
			new DocumentDetail("Infrastructure Development Plan 20XX")
		});

			// (4) EMPLOYMENT
			cat4.Documents.AddRange(new[]
			{
			new DocumentDetail("Recruitment and Selection Policy and Procedure"), new DocumentDetail("Employee Compensation and Grading Structure"),
			new DocumentDetail("Workplace Diversity and Inclusion Program Document"), new DocumentDetail("Employee Health and Wellness Policy"),
			new DocumentDetail("Employee Disciplinary Action Form and Protocol"), new DocumentDetail("Annual Employee Training Needs Assessment"),
			new DocumentDetail("Internal Communication Policy for Employees"), new DocumentDetail("Policy on Work-Life Balance and Flexibility"),
			new DocumentDetail("Exit Interview Process and Analysis Report"), new DocumentDetail("Job Description and Specification Master File"),
			new DocumentDetail("Employee Performance Management System Guide"), new DocumentDetail("Workplace Harassment and Bullying Prevention Policy"),
			new DocumentDetail("Employee Grievance Handling Flowchart"), new DocumentDetail("Employee Onboarding and Induction Manual"),
			new DocumentDetail("Workforce Planning and Budget Document")
		});

			// (5) DISPUTES
			cat5.Documents.AddRange(new[]
			{
			new DocumentDetail("Legal Dispute Management Protocol"), new DocumentDetail("Litigation Risk Assessment Report"),
			new DocumentDetail("Dispute Resolution Strategy Document"), new DocumentDetail("External Legal Counsel Engagement Guidelines"),
			new DocumentDetail("Dispute Case Files Index and Tracking Log"), new DocumentDetail("Confidentiality Agreement for Dispute Information"),
			new DocumentDetail("Settlement Agreement Standard Template"), new DocumentDetail("Internal Reporting Procedure for Potential Litigation"),
			new DocumentDetail("Cost-Benefit Analysis for Dispute Progression"), new DocumentDetail("Legal Hold and Document Preservation Policy"),
			new DocumentDetail("Arbitration and Mediation Procedure Manual"), new DocumentDetail("Witness Interview and Deposition Preparation Guide"),
			new DocumentDetail("Historical Dispute Resolution Summary"), new DocumentDetail("Attorney-Client Privilege Documentation Guide"),
			new DocumentDetail("Dispute Escalation Matrix")
		});

			// (5.1) ON GOING DISPUTES
			cat5_1.Documents.AddRange(new[]
			{
			new DocumentDetail("Active Litigation Case Briefs 20XX"), new DocumentDetail("Current Dispute Status and Update Report"),
			new DocumentDetail("Legal Correspondence Log for Active Cases"), new DocumentDetail("Court Filings and Pleadings Repository Index"),
			new DocumentDetail("On-Going Dispute Budget and Expenditure Tracker"), new DocumentDetail("Settlement Negotiation Strategy for Case A"),
			new DocumentDetail("Witness Testimony Summary for Current Cases"), new DocumentDetail("Expert Witness Engagement Documents"),
			new DocumentDetail("Internal Review Minutes on Active Dispute Strategy"), new DocumentDetail("On-Going Dispute Risk Exposure Report"),
			new DocumentDetail("Judgement and Order Compliance Checklist"), new DocumentDetail("Case Management Timeline and Milestones"),
			new DocumentDetail("Legal Opinion Summary for Active Matters"), new DocumentDetail("Insurance Claim Documents for Litigation"),
			new DocumentDetail("Dispute Team Communication Protocol")
		});

			// (5.2) POTENTIAL DISPUTES
			cat5_2.Documents.AddRange(new[]
			{
			new DocumentDetail("Potential Litigation Risk Assessment Report"), new DocumentDetail("Pre-Dispute Demand Letter and Response Template"),
			new DocumentDetail("Internal Memo on Matter X Legal Exposure"), new DocumentDetail("Early Case Assessment (ECA) Protocol"),
			new DocumentDetail("Document Collection and Preservation Plan for Matter Y"), new DocumentDetail("Confidentiality Undertaking for Pre-Litigation Discussions"),
			new DocumentDetail("External Legal Opinion on Potential Claim"), new DocumentDetail("Communication Strategy for Affected Parties"),
			new DocumentDetail("Potential Dispute Budget Allocation and Tracking"), new DocumentDetail("Settlement Authority and Approval Document"),
			new DocumentDetail("Summary of Pre-Litigation Negotiations"), new DocumentDetail("Witness Identification and Interview Records"),
			new DocumentDetail("Statute of Limitations Tracking Log"), new DocumentDetail("Legal Hold Notice Issuance Protocol"),
			new DocumentDetail("Historical Review of Similar Potential Claims")
		});

			// (6) REGULATORY ISSUES
			cat6.Documents.AddRange(new[]
			{
			new DocumentDetail("Regulatory Compliance Manual and Monitoring Plan"), new DocumentDetail("Key Regulatory Register and Impact Assessment"),
			new DocumentDetail("Licensing and Permit Renewal Schedule"), new DocumentDetail("Internal Audit Findings and Action Plan for Compliance"),
			new DocumentDetail("Regulator Communication Protocol"), new DocumentDetail("Data Privacy and Protection Policy (e.g., GDPR/local law)"),
			new DocumentDetail("Environmental Regulations Compliance Report"), new DocumentDetail("Industry-Specific Regulation Checklist"),
			new DocumentDetail("Policy on Handling Regulatory Inspections"), new DocumentDetail("Anti-Money Laundering (AML) Compliance Document"),
			new DocumentDetail("Regulatory Change Management Procedure"), new DocumentDetail("Compliance Training Materials for Employees"),
			new DocumentDetail("Summary of Legislative Updates 20XX"), new DocumentDetail("Non-Compliance Incident Reporting Form"),
			new DocumentDetail("Regulatory Filing and Disclosure Calendar")
		});

			// (7) HUMAN RESOURCES (HR)
			cat7.Documents.AddRange(new[]
			{
			new DocumentDetail("Consolidated HR Master Policy Document"), new DocumentDetail("Employee Relations and Engagement Strategy"),
			new DocumentDetail("HR Information System (HRIS) Data Management Protocol"), new DocumentDetail("Talent Acquisition and Selection Procedure"),
			new DocumentDetail("Annual Workforce Planning Document"), new DocumentDetail("Employee Benefits Enrollment Guide"),
			new DocumentDetail("Disciplinary and Termination Policy Detailed Manual"), new DocumentDetail("HR Budget and Financial Plan 20XX"),
			new DocumentDetail("Diversity, Equity, and Inclusion (DEI) Policy"), new DocumentDetail("Training Program Effectiveness Report"),
			new DocumentDetail("Occupational Health Services Agreement"), new DocumentDetail("Succession Planning and Talent Review Minutes"),
			new DocumentDetail("Remote Work and Flexible Hours Policy"), new DocumentDetail("HR Audit Checklist and Action Plan"),
			new DocumentDetail("Employee Handbook Acknowledgement Form")
		});

			return topLevelCategories;
		}
	}
}
