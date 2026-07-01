using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Repositories;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Services
{
    public class ModuleCsvService : IModuleCsvService
    {
        private readonly MemberRepository _memberRepo;
        private readonly StaffRepository _staffRepo;
        private readonly LeadRepository _leadRepo;
        private readonly MembershipRepository _membershipRepo;
        private readonly ExerciseRepository _exerciseRepo;
        private readonly DietRepository _dietRepo;
        private readonly ClassRepository _classRepo;
        private readonly EquipmentRepository _equipmentRepo;
        private readonly ProductRepository _productRepo;
        private readonly VendorRepository _vendorRepo;
        private readonly ExpenseHeadRepository _expenseHeadRepo;
        private readonly LeadSourceRepository _leadSourceRepo;
        private readonly AttendanceRepository _attendanceRepo;
        private readonly StaffAttendanceRepository _staffAttendanceRepo;
        private readonly PaymentRepository _paymentRepo;
        private readonly ExpenseRepository _expenseRepo;
        private readonly MembershipTransactionRepository _membershipTxnRepo;
        private readonly MembersRepository _membersRepo;
        private readonly ClassBookingRepository _classBookingRepo;
        private readonly StockPurchaseRepository _stockPurchaseRepo;
        private readonly StockIssueRepository _stockIssueRepo;
        private readonly EquipmentMaintenanceRepository _equipmentMaintenanceRepo;
        private readonly PTSessionRepository _ptSessionRepo;
        private readonly TrainerAssignmentRepository _trainerAssignmentRepo;
        private readonly SalaryProcessingRepository _salaryProcessingRepo;
        private readonly WorkoutPlanRepository _workoutPlanRepo;
        private readonly DietPlanRepository _dietPlanRepo;
        private readonly SalaryRuleMasterRepository _salaryRuleRepo;
        private readonly ReportRepository _reportRepo;

        public ModuleCsvService(
            MemberRepository memberRepo,
            StaffRepository staffRepo,
            LeadRepository leadRepo,
            MembershipRepository membershipRepo,
            ExerciseRepository exerciseRepo,
            DietRepository dietRepo,
            ClassRepository classRepo,
            EquipmentRepository equipmentRepo,
            ProductRepository productRepo,
            VendorRepository vendorRepo,
            ExpenseHeadRepository expenseHeadRepo,
            LeadSourceRepository leadSourceRepo,
            AttendanceRepository attendanceRepo,
            StaffAttendanceRepository staffAttendanceRepo,
            PaymentRepository paymentRepo,
            ExpenseRepository expenseRepo,
            MembershipTransactionRepository membershipTxnRepo,
            MembersRepository membersRepo,
            ClassBookingRepository classBookingRepo,
            StockPurchaseRepository stockPurchaseRepo,
            StockIssueRepository stockIssueRepo,
            EquipmentMaintenanceRepository equipmentMaintenanceRepo,
            PTSessionRepository ptSessionRepo,
            TrainerAssignmentRepository trainerAssignmentRepo,
            SalaryProcessingRepository salaryProcessingRepo,
            WorkoutPlanRepository workoutPlanRepo,
            DietPlanRepository dietPlanRepo,
            SalaryRuleMasterRepository salaryRuleRepo,
            ReportRepository reportRepo)
        {
            _memberRepo = memberRepo;
            _staffRepo = staffRepo;
            _leadRepo = leadRepo;
            _membershipRepo = membershipRepo;
            _exerciseRepo = exerciseRepo;
            _dietRepo = dietRepo;
            _classRepo = classRepo;
            _equipmentRepo = equipmentRepo;
            _productRepo = productRepo;
            _vendorRepo = vendorRepo;
            _expenseHeadRepo = expenseHeadRepo;
            _leadSourceRepo = leadSourceRepo;
            _attendanceRepo = attendanceRepo;
            _staffAttendanceRepo = staffAttendanceRepo;
            _paymentRepo = paymentRepo;
            _expenseRepo = expenseRepo;
            _membershipTxnRepo = membershipTxnRepo;
            _membersRepo = membersRepo;
            _classBookingRepo = classBookingRepo;
            _stockPurchaseRepo = stockPurchaseRepo;
            _stockIssueRepo = stockIssueRepo;
            _equipmentMaintenanceRepo = equipmentMaintenanceRepo;
            _ptSessionRepo = ptSessionRepo;
            _trainerAssignmentRepo = trainerAssignmentRepo;
            _salaryProcessingRepo = salaryProcessingRepo;
            _workoutPlanRepo = workoutPlanRepo;
            _dietPlanRepo = dietPlanRepo;
            _salaryRuleRepo = salaryRuleRepo;
            _reportRepo = reportRepo;
        }

        public CsvFileData BuildExport(string module, IQueryCollection query)
        {
            return module switch
            {
                "MemberMaster" => ExportMemberMaster(query),
                "StaffMaster" => ExportStaffMaster(query),
                "Leads" => ExportLeads(query),
                "MembershipPlans" => ExportMembershipPlans(query),
                "ExerciseMaster" => ExportExerciseMaster(query),
                "DietMaster" => ExportDietMaster(query),
                "Classes" => ExportClasses(query),
                "Equipment" => ExportEquipment(query),
                "Products" => ExportProducts(query),
                "Vendors" => ExportVendors(query),
                "ExpenseHeads" => ExportExpenseHeads(query),
                "LeadSources" => ExportLeadSources(query),
                "Attendance" => ExportAttendance(query),
                "StaffAttendance" => ExportStaffAttendance(query),
                "Payments" => ExportPayments(query),
                "Expenses" => ExportExpenses(query),
                "MembershipManagement" => ExportMembershipManagement(query),
                "Members" => ExportMembers(query),
                "ClassBookings" => ExportClassBookings(query),
                "StockPurchase" => ExportStockPurchase(query),
                "StockIssue" => ExportStockIssue(query),
                "EquipmentMaintenance" => ExportEquipmentMaintenance(query),
                "PTSessions" => ExportPTSessions(query),
                "TrainerAssignment" => ExportTrainerAssignment(query),
                "SalaryProcessing" => ExportSalaryProcessing(query),
                "SalaryStatement" => ExportSalaryProcessing(query),
                "BankStatement" => ExportBankStatementCsv(query),
                "WorkoutPlans" => ExportWorkoutPlans(query),
                "DietPlans" => ExportDietPlans(query),
                "SalaryRuleMaster" => ExportSalaryRuleMaster(query),
                "ReportAttendance" => ExportReportAttendance(query),
                "ReportExpiry" => ExportReportExpiry(query),
                "ReportCollections" => ExportReportCollections(query),
                "ReportOutstanding" => ExportReportOutstanding(query),
                "ReportProfitLoss" => ExportReportProfitLoss(query),
                _ => throw new ArgumentException($"Unknown export module: {module}", nameof(module))
            };
        }

        public CsvImportResult Import(string module, Stream stream)
        {
            return module switch
            {
                "MemberMaster" => ImportMemberMaster(stream),
                "StaffMaster" => ImportStaffMaster(stream),
                "Leads" => ImportLeads(stream),
                "MembershipPlans" => ImportMembershipPlans(stream),
                "ExerciseMaster" => ImportExerciseMaster(stream),
                "DietMaster" => ImportDietMaster(stream),
                "Classes" => ImportClasses(stream),
                "Equipment" => ImportEquipment(stream),
                "Products" => ImportProducts(stream),
                "Vendors" => ImportVendors(stream),
                "ExpenseHeads" => ImportExpenseHeads(stream),
                "LeadSources" => ImportLeadSources(stream),
                "SalaryRuleMaster" => ImportSalaryRuleMaster(stream),
                "SalaryProcessing" => ImportSalaryProcessing(stream),
                "SalaryStatement" => ImportSalaryProcessing(stream),
                "BankStatement" => ImportSalaryProcessing(stream),
                _ => new CsvImportResult
                {
                    FailedCount = 1,
                    Errors = { "Import is not supported for this module." }
                }
            };
        }

        #region Export — Masters

        private CsvFileData ExportMemberMaster(IQueryCollection query)
        {
            var items = _memberRepo.GetAll(Q(query, "search"), Q(query, "status"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "MobileNo", "Email", "Gender", "Address",
                "TrainerName", "PlanName", "JoinDate", "PlanStartDate", "PlanEndDate", "Status"
            };
            return MakeFile("member-master.csv", headers, items.Select(m => new string?[]
            {
                m.MemberCode, m.MemberName, m.MobileNo, m.Email, m.Gender, m.Address,
                m.TrainerName, m.PlanName, D(m.JoinDate), D(m.PlanStartDate), D(m.PlanEndDate), m.Status
            }));
        }

        private CsvFileData ExportStaffMaster(IQueryCollection query)
        {
            var items = _staffRepo.GetAll();
            var headers = new[]
            {
                "StaffCode", "StaffName", "Gender", "MobileNo", "Email", "Designation", "Specializations",
                "ExperienceYears", "Salary", "JoiningDate", "Address", "BankName", "BankAccountNo", "IfscCode",
                "IsActive", "RoleId", "RoleName"
            };
            return MakeFile("staff-master.csv", headers, items.Select(s => new string?[]
            {
                s.StaffCode ?? s.DisplayStaffCode, s.StaffName, s.Gender, s.MobileNo, s.Email, s.Designation, s.Specializations,
                S(s.ExperienceYears), S(s.Salary), D(s.JoiningDate), s.Address, s.BankName, s.BankAccountNo, s.IfscCode,
                B(s.IsActive), S(s.RoleId), s.RoleName
            }));
        }

        private CsvFileData ExportLeads(IQueryCollection query)
        {
            var items = _leadRepo.GetAll(
                Q(query, "search"),
                Q(query, "status"),
                QInt(query, "sourceId"),
                QInt(query, "assignedTo"));
            var headers = new[]
            {
                "LeadCode", "LeadName", "MobileNo", "AlternateMobile", "Email", "Gender", "Address",
                "InterestedIn", "LeadSourceId", "LeadSourceName", "AssignedTo", "AssignedStaffName",
                "Status", "Budget", "FollowUpDate", "Remarks", "IsConverted", "IsActive"
            };
            return MakeFile("leads.csv", headers, items.Select(l => new string?[]
            {
                l.LeadCode, l.LeadName, l.MobileNo, l.AlternateMobile, l.Email, l.Gender, l.Address,
                l.InterestedIn, S(l.LeadSourceId), l.LeadSourceName, S(l.AssignedTo), l.AssignedStaffName,
                l.Status, S(l.Budget), D(l.FollowUpDate), l.Remarks, B(l.IsConverted), B(l.IsActive)
            }));
        }

        private CsvFileData ExportMembershipPlans(IQueryCollection query)
        {
            var items = _membershipRepo.GetAll(Q(query, "search"), QBool(query, "status"));
            var headers = new[] { "PlanName", "DurationMonths", "Amount", "JoiningFee", "Description", "IsActive" };
            return MakeFile("membership-plans.csv", headers, items.Select(p => new string?[]
            {
                p.PlanName, S(p.DurationMonths), S(p.Amount), S(p.JoiningFee), p.Description, B(p.IsActive)
            }));
        }

        private CsvFileData ExportExerciseMaster(IQueryCollection query)
        {
            var items = _exerciseRepo.GetAll(Q(query, "search"), QBool(query, "status"));
            var headers = new[] { "ExerciseName", "MuscleGroup", "DifficultyLevel", "CaloriesBurn", "Description", "Status" };
            return MakeFile("exercise-master.csv", headers, items.Select(e => new string?[]
            {
                e.ExerciseName, e.MuscleGroup, e.DifficultyLevel, S(e.CaloriesBurn), e.Description, B(e.Status)
            }));
        }

        private CsvFileData ExportDietMaster(IQueryCollection query)
        {
            var items = _dietRepo.GetAll(Q(query, "search"), Q(query, "category"));
            var headers = new[]
            {
                "DietName", "Category", "MealType", "Calories", "Protein", "Carbs", "Fat", "Description", "IsActive"
            };
            return MakeFile("diet-master.csv", headers, items.Select(d => new string?[]
            {
                d.DietName, d.Category, d.MealType, S(d.Calories), S(d.Protein), S(d.Carbs), S(d.Fat),
                d.Description, B(d.IsActive)
            }));
        }

        private CsvFileData ExportClasses(IQueryCollection query)
        {
            var items = _classRepo.GetAll(Q(query, "search"), Q(query, "trainerId"));
            var headers = new[]
            {
                "ClassName", "TrainerId", "TrainerName", "Schedule", "StartTime", "EndTime",
                "MaxCapacity", "Amount", "IsActive"
            };
            return MakeFile("classes.csv", headers, items.Select(c => new string?[]
            {
                c.ClassName, S(c.TrainerId), c.TrainerName, c.Schedule, T(c.StartTime), T(c.EndTime),
                S(c.MaxCapacity), S(c.Amount), B(c.IsActive)
            }));
        }

        private CsvFileData ExportEquipment(IQueryCollection query)
        {
            var items = _equipmentRepo.GetAll(Q(query, "search"), Q(query, "conditionStatus"));
            var headers = new[]
            {
                "EquipmentName", "Category", "PurchaseDate", "PurchasePrice", "Quantity",
                "ConditionStatus", "Location", "Remarks", "IsActive"
            };
            return MakeFile("equipment.csv", headers, items.Select(e => new string?[]
            {
                e.EquipmentName, e.Category, D(e.PurchaseDate), S(e.PurchasePrice), S(e.Quantity),
                e.ConditionStatus, e.Location, e.Remarks, B(e.IsActive)
            }));
        }

        private CsvFileData ExportProducts(IQueryCollection query)
        {
            var items = _productRepo.GetAll(Q(query, "search"), Q(query, "category"));
            var headers = new[]
            {
                "ProductName", "Category", "UnitPrice", "CurrentStock", "ReorderLevel", "VendorId", "VendorName", "IsActive"
            };
            return MakeFile("products.csv", headers, items.Select(p => new string?[]
            {
                p.ProductName, p.Category, S(p.UnitPrice), S(p.CurrentStock), S(p.ReorderLevel),
                S(p.VendorId), p.VendorName, B(p.IsActive)
            }));
        }

        private CsvFileData ExportVendors(IQueryCollection query)
        {
            var items = _vendorRepo.GetAll(Q(query, "search"));
            var headers = new[] { "VendorName", "ContactPerson", "MobileNo", "Email", "Address", "GSTNo", "IsActive" };
            return MakeFile("vendors.csv", headers, items.Select(v => new string?[]
            {
                v.VendorName, v.ContactPerson, v.MobileNo, v.Email, v.Address, v.GSTNo, B(v.IsActive)
            }));
        }

        private CsvFileData ExportExpenseHeads(IQueryCollection query)
        {
            var items = _expenseHeadRepo.GetAll(Q(query, "search"));
            var headers = new[] { "HeadName", "Description", "IsActive" };
            return MakeFile("expense-heads.csv", headers, items.Select(h => new string?[]
            {
                h.HeadName, h.Description, B(h.IsActive)
            }));
        }

        private CsvFileData ExportLeadSources(IQueryCollection query)
        {
            var items = _leadSourceRepo.GetAll(Q(query, "search"));
            var headers = new[] { "SourceCode", "SourceName", "Description", "DisplayOrder", "IsActive" };
            return MakeFile("lead-sources.csv", headers, items.Select(s => new string?[]
            {
                s.SourceCode, s.SourceName, s.Description, S(s.DisplayOrder), B(s.IsActive)
            }));
        }

        #endregion

        #region Export — Transactions

        private CsvFileData ExportAttendance(IQueryCollection query)
        {
            var items = _attendanceRepo.GetAll(QDate(query, "filterDate"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "AttendanceDate", "CheckInTime", "CheckOutTime", "Remarks"
            };
            return MakeFile("attendance.csv", headers, items.Select(a => new string?[]
            {
                a.MemberCode, a.MemberName, D(a.AttendanceDate), T(a.CheckInTime), T(a.CheckOutTime), a.Remarks
            }));
        }

        private CsvFileData ExportStaffAttendance(IQueryCollection query)
        {
            var items = _staffAttendanceRepo.GetAll(
                Q(query, "search"),
                Q(query, "staffId"),
                Q(query, "status"),
                Q(query, "fromDate"),
                Q(query, "toDate"));
            var headers = new[]
            {
                "StaffName", "Designation", "AttendanceDate", "CheckInTime", "CheckOutTime", "Status", "Remarks"
            };
            return MakeFile("staff-attendance.csv", headers, items.Select(a => new string?[]
            {
                a.StaffName, a.Designation, D(a.AttendanceDate), T(a.CheckInTime), T(a.CheckOutTime), a.Status, a.Remarks
            }));
        }

        private CsvFileData ExportPayments(IQueryCollection query)
        {
            var items = _paymentRepo.GetAll(QDate(query, "fromDate"), QDate(query, "toDate"), Q(query, "mode"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "PaymentDate", "Amount", "PaymentMode", "ReferenceNo", "Remarks"
            };
            return MakeFile("payments.csv", headers, items.Select(p => new string?[]
            {
                p.MemberCode, p.MemberName, D(p.PaymentDate), S(p.Amount), p.PaymentMode, p.ReferenceNo, p.Remarks
            }));
        }

        private CsvFileData ExportExpenses(IQueryCollection query)
        {
            var items = _expenseRepo.GetAll(
                Q(query, "search"),
                Q(query, "expenseHeadId"),
                Q(query, "paymentMode"),
                Q(query, "fromDate"),
                Q(query, "toDate"));
            var headers = new[]
            {
                "HeadName", "Amount", "ExpenseDate", "Description", "PaymentMode", "PaidTo", "Remarks"
            };
            return MakeFile("expenses.csv", headers, items.Select(e => new string?[]
            {
                e.HeadName, S(e.Amount), D(e.ExpenseDate), e.Description, e.PaymentMode, e.PaidTo, e.Remarks
            }));
        }

        private CsvFileData ExportMembershipManagement(IQueryCollection query)
        {
            var items = _membershipTxnRepo.GetAll();
            var headers = new[]
            {
                "MemberName", "PlanName", "StartDate", "EndDate", "Amount", "PaymentStatus", "MembershipStatus", "Remarks"
            };
            return MakeFile("membership-management.csv", headers, items.Select(t => new string?[]
            {
                t.MemberName, t.PlanName, D(t.StartDate), D(t.EndDate), S(t.Amount),
                t.PaymentStatus, t.MembershipStatus, t.Remarks
            }));
        }

        private CsvFileData ExportMembers(IQueryCollection query)
        {
            var items = _membersRepo.GetAll(
                Q(query, "search"),
                Q(query, "trainer"),
                Q(query, "plan"),
                Q(query, "status"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "MobileNo", "Status", "JoinDate", "TrainerName", "PlanName"
            };
            return MakeFile("members.csv", headers, items.Select(m => new string?[]
            {
                m.MemberCode, m.MemberName, m.MobileNo, m.Status, D(m.JoinDate), m.TrainerName, m.PlanName
            }));
        }

        private CsvFileData ExportClassBookings(IQueryCollection query)
        {
            var items = _classBookingRepo.GetAll(
                Q(query, "search"),
                Q(query, "memberId"),
                Q(query, "classId"),
                Q(query, "status"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "ClassName", "TrainerName", "BookingDate", "StartTime", "EndTime",
                "Status", "Amount", "Remarks"
            };
            return MakeFile("class-bookings.csv", headers, items.Select(b => new string?[]
            {
                b.MemberCode, b.MemberName, b.ClassName, b.TrainerName, D(b.BookingDate),
                T(b.StartTime), T(b.EndTime), b.Status, S(b.Amount), b.Remarks
            }));
        }

        private CsvFileData ExportStockPurchase(IQueryCollection query)
        {
            var items = _stockPurchaseRepo.GetAll(
                Q(query, "search"),
                Q(query, "productId"),
                Q(query, "vendorId"),
                Q(query, "fromDate"),
                Q(query, "toDate"));
            var headers = new[]
            {
                "ProductName", "Category", "VendorName", "Quantity", "UnitPrice", "TotalAmount",
                "PurchaseDate", "InvoiceNo", "Remarks"
            };
            return MakeFile("stock-purchase.csv", headers, items.Select(p => new string?[]
            {
                p.ProductName, p.Category, p.VendorName, S(p.Quantity), S(p.UnitPrice), S(p.TotalAmount),
                D(p.PurchaseDate), p.InvoiceNo, p.Remarks
            }));
        }

        private CsvFileData ExportStockIssue(IQueryCollection query)
        {
            var items = _stockIssueRepo.GetAll(
                Q(query, "search"),
                Q(query, "productId"),
                Q(query, "memberId"),
                Q(query, "fromDate"),
                Q(query, "toDate"));
            var headers = new[]
            {
                "ProductName", "Category", "MemberCode", "MemberName", "Quantity", "IssueDate",
                "IssuedTo", "Amount", "PaymentMode", "Remarks"
            };
            return MakeFile("stock-issue.csv", headers, items.Select(i => new string?[]
            {
                i.ProductName, i.Category, i.MemberCode, i.MemberName, S(i.Quantity), D(i.IssueDate),
                i.IssuedTo, S(i.Amount), i.PaymentMode, i.Remarks
            }));
        }

        private CsvFileData ExportEquipmentMaintenance(IQueryCollection query)
        {
            var items = _equipmentMaintenanceRepo.GetAll(
                Q(query, "search"),
                Q(query, "equipmentId"),
                Q(query, "status"),
                Q(query, "fromDate"),
                Q(query, "toDate"));
            var headers = new[]
            {
                "EquipmentName", "Category", "Location", "MaintenanceDate", "MaintenanceType", "Cost",
                "PaymentMode", "VendorName", "NextDueDate", "Status", "Remarks"
            };
            return MakeFile("equipment-maintenance.csv", headers, items.Select(m => new string?[]
            {
                m.EquipmentName, m.Category, m.Location, D(m.MaintenanceDate), m.MaintenanceType, S(m.Cost),
                m.PaymentMode, m.VendorName, D(m.NextDueDate), m.Status, m.Remarks
            }));
        }

        private CsvFileData ExportPTSessions(IQueryCollection query)
        {
            var items = _ptSessionRepo.GetAll(Q(query, "search"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "TrainerName", "SessionDate", "StartTime", "EndTime", "Status", "Remarks"
            };
            return MakeFile("pt-sessions.csv", headers, items.Select(s => new string?[]
            {
                s.MemberCode, s.MemberName, s.TrainerName, D(s.SessionDate),
                T(s.StartTime), T(s.EndTime), s.Status, s.Remarks
            }));
        }

        private CsvFileData ExportTrainerAssignment(IQueryCollection query)
        {
            var items = _trainerAssignmentRepo.GetAll(Q(query, "search"), QBool(query, "status"));
            var headers = new[]
            {
                "MemberName", "TrainerName", "StartDate", "EndDate", "IsActive", "Remarks"
            };
            return MakeFile("trainer-assignment.csv", headers, items.Select(a => new string?[]
            {
                a.MemberName, a.TrainerName, D(a.StartDate), D(a.EndDate), B(a.IsActive), a.Remarks
            }));
        }

        private CsvFileData ExportSalaryProcessing(IQueryCollection query)
        {
            var items = _salaryProcessingRepo.GetAll(
                Q(query, "search"),
                Q(query, "staffId"),
                Q(query, "month"),
                Q(query, "year"));
            var headers = new[]
            {
                "StaffCode", "StaffName", "Designation", "Month", "Year", "BasicSalary", "Deductions", "NetSalary",
                "PresentDays", "AbsentDays", "LeaveDays", "HalfDays", "PaidDate", "PaymentMode", "Remarks"
            };
            return MakeFile("salary-processing.csv", headers, items.Select(s => new string?[]
            {
                s.DisplayStaffCode, s.StaffName, s.Designation, S(s.Month), S(s.Year), S(s.BasicSalary), S(s.Deductions), S(s.NetSalary),
                S(s.PresentDays), S(s.AbsentDays), S(s.LeaveDays), S(s.HalfDays),
                D(s.PaidDate), s.PaymentMode, s.Remarks
            }));
        }

        private CsvFileData ExportBankStatementCsv(IQueryCollection query)
        {
            var month = int.TryParse(Q(query, "month"), out var m) ? m : DateTime.Today.Month;
            var year = int.TryParse(Q(query, "year"), out var y) ? y : DateTime.Today.Year;
            var items = _salaryProcessingRepo.GetAll(null, null, month.ToString(), year.ToString());
            var headers = new[]
            {
                "StaffCode", "StaffName", "Month", "Year", "NetSalary", "PaidDate", "PaymentMode", "Remarks"
            };
            return MakeFile($"bank-statement-{month:00}-{year}.csv", headers, items.Select(s => new string?[]
            {
                s.DisplayStaffCode, s.StaffName, S(s.Month), S(s.Year), S(s.NetSalary),
                D(s.PaidDate), s.PaymentMode, s.Remarks
            }));
        }

        private CsvFileData ExportWorkoutPlans(IQueryCollection query)
        {
            var items = _workoutPlanRepo.GetAll(Q(query, "search"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "TrainerName", "PlanName", "StartDate", "EndDate", "Goals", "Remarks"
            };
            return MakeFile("workout-plans.csv", headers, items.Select(p => new string?[]
            {
                p.MemberCode, p.MemberName, p.TrainerName, p.PlanName,
                D(p.StartDate), D(p.EndDate), p.Goals, p.Remarks
            }));
        }

        private CsvFileData ExportDietPlans(IQueryCollection query)
        {
            var items = _dietPlanRepo.GetAll(Q(query, "search"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "PlanName", "StartDate", "EndDate", "CalorieTarget", "Remarks"
            };
            return MakeFile("diet-plans.csv", headers, items.Select(p => new string?[]
            {
                p.MemberCode, p.MemberName, p.PlanName, D(p.StartDate), D(p.EndDate), S(p.CalorieTarget), p.Remarks
            }));
        }

        private CsvFileData ExportSalaryRuleMaster(IQueryCollection query)
        {
            var items = _salaryRuleRepo.GetAll(Q(query, "search"));
            var headers = new[]
            {
                "RuleName", "WorkingDaysPerMonth", "AbsentDeductionPerDay", "HalfDayDeductionFactor",
                "LeaveIsPaid", "LateGraceMinutes", "ShiftStartTime", "ShiftEndTime", "EarlyLeaveGraceMinutes",
                "EnableSandwichRule", "WeeklyOffDays", "LateCountsAsHalfDay", "EarlyLeaveCountsAsHalfDay",
                "Description", "IsActive", "IsDefault"
            };
            return MakeFile("salary-rule-master.csv", headers, items.Select(r => new string?[]
            {
                r.RuleName, S(r.WorkingDaysPerMonth), B(r.AbsentDeductionPerDay), S(r.HalfDayDeductionFactor),
                B(r.LeaveIsPaid), S(r.LateGraceMinutes), T(r.ShiftStartTime), T(r.ShiftEndTime),
                S(r.EarlyLeaveGraceMinutes), B(r.EnableSandwichRule), r.WeeklyOffDays,
                B(r.LateCountsAsHalfDay), B(r.EarlyLeaveCountsAsHalfDay),
                r.Description, B(r.IsActive), B(r.IsDefault)
            }));
        }

        #endregion

        #region Export — Reports

        private CsvFileData ExportReportAttendance(IQueryCollection query)
        {
            var from = QDate(query, "fromDate") ?? DateTime.Today.AddDays(-30);
            var to = QDate(query, "toDate") ?? DateTime.Today;
            if (from > to) (from, to) = (to, from);

            var model = _reportRepo.GetAttendanceReport(from, to);
            var headers = new[]
            {
                "MemberCode", "MemberName", "AttendanceDate", "CheckInTime", "CheckOutTime", "Remarks"
            };
            return MakeFile("report-attendance.csv", headers, model.Rows.Select(r => new string?[]
            {
                r.MemberCode, r.MemberName, D(r.AttendanceDate), T(r.CheckInTime), T(r.CheckOutTime), r.Remarks
            }));
        }

        private CsvFileData ExportReportExpiry(IQueryCollection query)
        {
            int days = QInt(query, "daysAhead") ?? 30;
            if (days < 1) days = 1;
            if (days > 365) days = 365;

            var model = _reportRepo.GetExpiryReport(days);
            var headers = new[]
            {
                "MemberCode", "MemberName", "MobileNo", "PlanName", "PlanEndDate", "DaysLeft", "Status"
            };
            return MakeFile("report-expiry.csv", headers, model.Rows.Select(r => new string?[]
            {
                r.MemberCode, r.MemberName, r.MobileNo, r.PlanName, D(r.PlanEndDate), S(r.DaysLeft), r.Status
            }));
        }

        private CsvFileData ExportReportCollections(IQueryCollection query)
        {
            var from = QDate(query, "fromDate") ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = QDate(query, "toDate") ?? DateTime.Today;
            if (from > to) (from, to) = (to, from);

            var model = _reportRepo.GetCollectionsReport(from, to, Q(query, "mode"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "PaymentDate", "Amount", "PaymentMode", "ReferenceNo"
            };
            return MakeFile("report-collections.csv", headers, model.Rows.Select(r => new string?[]
            {
                r.MemberCode, r.MemberName, D(r.PaymentDate), S(r.Amount), r.PaymentMode, r.ReferenceNo
            }));
        }

        private CsvFileData ExportReportOutstanding(IQueryCollection query)
        {
            var model = _reportRepo.GetOutstandingReport(Q(query, "status"));
            var headers = new[]
            {
                "MemberCode", "MemberName", "MobileNo", "PlanName", "Amount", "PaymentStatus", "EndDate"
            };
            return MakeFile("report-outstanding.csv", headers, model.Rows.Select(r => new string?[]
            {
                r.MemberCode, r.MemberName, r.MobileNo, r.PlanName, S(r.Amount), r.PaymentStatus, D(r.EndDate)
            }));
        }

        private CsvFileData ExportReportProfitLoss(IQueryCollection query)
        {
            var from = QDate(query, "fromDate") ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = QDate(query, "toDate") ?? DateTime.Today;
            if (from > to) (from, to) = (to, from);

            var model = _reportRepo.GetProfitLossReport(from, to);
            var headers = new[] { "Section", "Label", "Value" };
            var rows = new List<IReadOnlyList<string?>>();

            rows.Add(new[] { "Summary", "From Date", D(from) });
            rows.Add(new[] { "Summary", "To Date", D(to) });
            rows.Add(new[] { "Summary", "Total Income", S(model.TotalIncome) });
            rows.Add(new[] { "Summary", "Total Expenses", S(model.TotalExpenses) });
            rows.Add(new[] { "Summary", "Total Salaries", S(model.TotalSalaries) });
            rows.Add(new[] { "Summary", "Net Profit", S(model.NetProfit) });
            rows.Add(BlankRow(3));

            rows.Add(new[] { "Income By Mode", "", "" });
            foreach (var row in model.IncomeByMode)
                rows.Add(new[] { "Income By Mode", row.Label, S(row.Amount) });
            rows.Add(BlankRow(3));

            rows.Add(new[] { "Expense Breakdown", "", "" });
            foreach (var row in model.ExpenseBreakdown)
                rows.Add(new[] { "Expense Breakdown", row.Label, S(row.Amount) });

            return MakeFile("report-profit-loss.csv", headers, rows);
        }

        #endregion

        #region Import — Masters

        private CsvImportResult ImportMemberMaster(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("MemberName", "Name");
                var mobile = row.GetValue("MobileNo", "Mobile", "Phone");
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(mobile))
                    return $"Row {line}: MemberName and MobileNo are required.";

                var joinDate = row.GetDate("JoinDate") ?? DateTime.Today;
                var member = new MemberMaster
                {
                    MemberName = name,
                    MobileNo = mobile,
                    AlternateMobile = NullIfEmpty(row.GetValue("AlternateMobile")),
                    Email = NullIfEmpty(row.GetValue("Email")),
                    Gender = NullIfEmpty(row.GetValue("Gender")),
                    DateOfBirth = row.GetDate("DateOfBirth", "DOB"),
                    BloodGroup = NullIfEmpty(row.GetValue("BloodGroup")),
                    Address = NullIfEmpty(row.GetValue("Address")),
                    City = NullIfEmpty(row.GetValue("City")),
                    State = NullIfEmpty(row.GetValue("State")),
                    Pincode = NullIfEmpty(row.GetValue("Pincode")),
                    EmergencyContact = NullIfEmpty(row.GetValue("EmergencyContact")),
                    EmergencyContactName = NullIfEmpty(row.GetValue("EmergencyContactName")),
                    TrainerId = row.GetInt("TrainerId"),
                    PlanId = row.GetInt("PlanId"),
                    JoinDate = joinDate,
                    PlanStartDate = row.GetDate("PlanStartDate") ?? joinDate,
                    PlanEndDate = row.GetDate("PlanEndDate") ?? joinDate.AddMonths(1),
                    Height = row.GetDecimal("Height") ?? 0,
                    Weight = row.GetDecimal("Weight") ?? 0,
                    Status = NullIfEmpty(row.GetValue("Status")) ?? "Active",
                    MedicalNotes = NullIfEmpty(row.GetValue("MedicalNotes")),
                    Remarks = NullIfEmpty(row.GetValue("Remarks"))
                };

                _memberRepo.Insert(member);
                return null;
            });

        private CsvImportResult ImportStaffMaster(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("StaffName", "Name");
                var mobile = row.GetValue("MobileNo", "Mobile", "Phone");
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(mobile))
                    return $"Row {line}: StaffName and MobileNo are required.";

                var staff = new StaffMaster
                {
                    StaffCode = NullIfEmpty(row.GetValue("StaffCode", "EmpCode", "EmployeeCode")),
                    StaffName = name,
                    MobileNo = mobile,
                    Gender = row.GetValue("Gender") is { Length: > 0 } g ? g : "Other",
                    Email = row.GetValue("Email"),
                    Designation = row.GetValue("Designation"),
                    Specializations = NullIfEmpty(row.GetValue("Specializations")),
                    ExperienceYears = row.GetInt("ExperienceYears"),
                    Salary = row.GetDecimal("Salary") ?? 0,
                    JoiningDate = row.GetDate("JoiningDate") ?? DateTime.Today,
                    Address = row.GetValue("Address"),
                    BankName = NullIfEmpty(row.GetValue("BankName", "Bank")),
                    BankAccountNo = NullIfEmpty(row.GetValue("BankAccountNo", "Bank AC NO", "AccountNo")),
                    IfscCode = NullIfEmpty(row.GetValue("IfscCode", "IFSCCode", "IFSC")),
                    IsActive = row.GetBool(true, "IsActive", "Active"),
                    RoleId = row.GetInt("RoleId") ?? 3
                };

                if (!string.IsNullOrWhiteSpace(staff.StaffCode) && _staffRepo.IsStaffCodeTaken(staff.StaffCode))
                    return $"Row {line}: StaffCode '{staff.StaffCode}' already exists.";

                _staffRepo.Insert(staff);
                return null;
            });

        private CsvImportResult ImportLeads(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("LeadName", "Name");
                var mobile = row.GetValue("MobileNo", "Mobile", "Phone");
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(mobile))
                    return $"Row {line}: LeadName and MobileNo are required.";

                var leadSourceId = ResolveLeadSourceId(row);
                if (leadSourceId <= 0)
                    return $"Row {line}: No active lead source found.";

                var lead = new Lead
                {
                    LeadName = name,
                    MobileNo = mobile,
                    AlternateMobile = NullIfEmpty(row.GetValue("AlternateMobile")),
                    Email = NullIfEmpty(row.GetValue("Email")),
                    Gender = NullIfEmpty(row.GetValue("Gender")),
                    Address = NullIfEmpty(row.GetValue("Address")),
                    InterestedIn = NullIfEmpty(row.GetValue("InterestedIn")),
                    LeadSourceId = leadSourceId,
                    AssignedTo = row.GetInt("AssignedTo"),
                    Status = NullIfEmpty(row.GetValue("Status")) ?? "New",
                    Budget = row.GetDecimal("Budget"),
                    Remarks = NullIfEmpty(row.GetValue("Remarks")),
                    FollowUpDate = row.GetDate("FollowUpDate"),
                    IsConverted = row.GetBool(false, "IsConverted"),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _leadRepo.Insert(lead);
                return null;
            });

        private CsvImportResult ImportMembershipPlans(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var planName = row.GetValue("PlanName", "Name");
                if (string.IsNullOrWhiteSpace(planName))
                    return $"Row {line}: PlanName is required.";

                var plan = new MembershipPlanMaster
                {
                    PlanName = planName,
                    DurationMonths = row.GetInt("DurationMonths") ?? 1,
                    Amount = row.GetDecimal("Amount") ?? 0,
                    JoiningFee = row.GetDecimal("JoiningFee") ?? 0,
                    Description = row.GetValue("Description"),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _membershipRepo.Insert(plan);
                return null;
            });

        private CsvImportResult ImportExerciseMaster(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("ExerciseName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: ExerciseName is required.";

                var exercise = new ExerciseMaster
                {
                    ExerciseName = name,
                    MuscleGroup = row.GetValue("MuscleGroup") is { Length: > 0 } mg ? mg : "General",
                    DifficultyLevel = row.GetValue("DifficultyLevel") is { Length: > 0 } dl ? dl : "Beginner",
                    CaloriesBurn = row.GetInt("CaloriesBurn") ?? 0,
                    Description = row.GetValue("Description"),
                    Status = row.GetBool(true, "Status", "IsActive", "Active")
                };

                _exerciseRepo.Insert(exercise);
                return null;
            });

        private CsvImportResult ImportDietMaster(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("DietName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: DietName is required.";

                var diet = new DietMaster
                {
                    DietName = name,
                    Category = row.GetValue("Category") is { Length: > 0 } c ? c : "General",
                    MealType = row.GetValue("MealType") is { Length: > 0 } mt ? mt : "Any",
                    Calories = row.GetDecimal("Calories"),
                    Protein = row.GetDecimal("Protein"),
                    Carbs = row.GetDecimal("Carbs"),
                    Fat = row.GetDecimal("Fat"),
                    Description = NullIfEmpty(row.GetValue("Description")),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _dietRepo.Insert(diet);
                return null;
            });

        private CsvImportResult ImportClasses(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("ClassName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: ClassName is required.";

                var cls = new ClassMaster
                {
                    ClassName = name,
                    TrainerId = row.GetInt("TrainerId"),
                    Schedule = NullIfEmpty(row.GetValue("Schedule")),
                    StartTime = row.GetTime("StartTime"),
                    EndTime = row.GetTime("EndTime"),
                    MaxCapacity = row.GetInt("MaxCapacity"),
                    Amount = row.GetDecimal("Amount"),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _classRepo.Insert(cls);
                return null;
            });

        private CsvImportResult ImportEquipment(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("EquipmentName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: EquipmentName is required.";

                var equipment = new EquipmentMaster
                {
                    EquipmentName = name,
                    Category = NullIfEmpty(row.GetValue("Category")),
                    PurchaseDate = row.GetDate("PurchaseDate"),
                    PurchasePrice = row.GetDecimal("PurchasePrice"),
                    Quantity = row.GetInt("Quantity") ?? 1,
                    ConditionStatus = row.GetValue("ConditionStatus") is { Length: > 0 } cs ? cs : "Good",
                    Location = NullIfEmpty(row.GetValue("Location")),
                    Remarks = NullIfEmpty(row.GetValue("Remarks")),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _equipmentRepo.Insert(equipment);
                return null;
            });

        private CsvImportResult ImportProducts(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("ProductName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: ProductName is required.";

                var product = new ProductMaster
                {
                    ProductName = name,
                    Category = NullIfEmpty(row.GetValue("Category")),
                    UnitPrice = row.GetDecimal("UnitPrice") ?? 0,
                    CurrentStock = row.GetInt("CurrentStock") ?? 0,
                    ReorderLevel = row.GetInt("ReorderLevel") ?? 0,
                    VendorId = row.GetInt("VendorId"),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _productRepo.Insert(product);
                return null;
            });

        private CsvImportResult ImportVendors(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("VendorName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: VendorName is required.";

                var vendor = new VendorMaster
                {
                    VendorName = name,
                    ContactPerson = NullIfEmpty(row.GetValue("ContactPerson")),
                    MobileNo = NullIfEmpty(row.GetValue("MobileNo", "Mobile")),
                    Email = NullIfEmpty(row.GetValue("Email")),
                    Address = NullIfEmpty(row.GetValue("Address")),
                    GSTNo = NullIfEmpty(row.GetValue("GSTNo")),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _vendorRepo.Insert(vendor);
                return null;
            });

        private CsvImportResult ImportExpenseHeads(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("HeadName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: HeadName is required.";

                var head = new ExpenseHeadMaster
                {
                    HeadName = name,
                    Description = NullIfEmpty(row.GetValue("Description")),
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _expenseHeadRepo.Insert(head);
                return null;
            });

        private CsvImportResult ImportLeadSources(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var name = row.GetValue("SourceName", "Name");
                if (string.IsNullOrWhiteSpace(name))
                    return $"Row {line}: SourceName is required.";

                var source = new LeadSourceMaster
                {
                    SourceName = name,
                    Description = NullIfEmpty(row.GetValue("Description")),
                    DisplayOrder = row.GetInt("DisplayOrder") ?? 0,
                    IsActive = row.GetBool(true, "IsActive", "Active")
                };

                _leadSourceRepo.Insert(source);
                return null;
            });

        private CsvImportResult ImportSalaryRuleMaster(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var ruleName = row.GetValue("RuleName", "Name");
                if (string.IsNullOrWhiteSpace(ruleName))
                    return $"Row {line}: RuleName is required.";

                var rule = new SalaryRuleMaster
                {
                    RuleName = ruleName,
                    WorkingDaysPerMonth = row.GetInt("WorkingDaysPerMonth") ?? 26,
                    AbsentDeductionPerDay = row.GetBool(true, "AbsentDeductionPerDay"),
                    HalfDayDeductionFactor = row.GetDecimal("HalfDayDeductionFactor") ?? 0.5m,
                    LeaveIsPaid = row.GetBool(true, "LeaveIsPaid"),
                    LateGraceMinutes = row.GetInt("LateGraceMinutes") ?? 15,
                    ShiftStartTime = row.GetTime("ShiftStartTime") ?? new TimeSpan(9, 0, 0),
                    ShiftEndTime = row.GetTime("ShiftEndTime") ?? new TimeSpan(18, 0, 0),
                    EarlyLeaveGraceMinutes = row.GetInt("EarlyLeaveGraceMinutes") ?? 0,
                    EnableSandwichRule = row.GetBool(false, "EnableSandwichRule"),
                    WeeklyOffDays = NullIfEmpty(row.GetValue("WeeklyOffDays")) ?? "Sunday",
                    LateCountsAsHalfDay = row.GetBool(true, "LateCountsAsHalfDay"),
                    EarlyLeaveCountsAsHalfDay = row.GetBool(true, "EarlyLeaveCountsAsHalfDay"),
                    Description = NullIfEmpty(row.GetValue("Description")),
                    IsActive = row.GetBool(true, "IsActive", "Active"),
                    IsDefault = row.GetBool(false, "IsDefault")
                };

                if (rule.WorkingDaysPerMonth <= 0)
                    rule.WorkingDaysPerMonth = 26;

                _salaryRuleRepo.Insert(rule);
                return null;
            });

        private CsvImportResult ImportSalaryProcessing(Stream stream) =>
            RunImport(stream, (row, line) =>
            {
                var staffId = ResolveStaffId(row);
                if (staffId <= 0)
                    return $"Row {line}: Staff not found. Use StaffId or StaffName (with Designation if duplicate names).";

                var month = row.GetInt("Month");
                var year = row.GetInt("Year");
                if (!month.HasValue || month < 1 || month > 12)
                    return $"Row {line}: Month (1–12) is required.";
                if (!year.HasValue || year < 2000)
                    return $"Row {line}: Year is required.";

                var existing = _salaryProcessingRepo.GetByStaffPeriod(staffId, month.Value, year.Value);

                var basic = row.GetDecimal("BasicSalary") ?? existing?.BasicSalary ?? 0;
                var deductions = row.GetDecimal("Deductions") ?? existing?.Deductions ?? 0;
                var net = row.GetDecimal("NetSalary") ?? (basic - deductions);

                if (basic <= 0 && net > 0)
                {
                    var staffMember = _staffRepo.GetById(staffId);
                    if (staffMember != null && staffMember.Salary > 0)
                        basic = staffMember.Salary;
                    else
                        basic = net;
                    deductions = Math.Max(0, basic - net);
                }

                var salary = new SalaryProcessing
                {
                    StaffId = staffId,
                    Month = month.Value,
                    Year = year.Value,
                    BasicSalary = basic,
                    Deductions = deductions,
                    NetSalary = net,
                    PresentDays = row.GetInt("PresentDays") ?? existing?.PresentDays,
                    AbsentDays = row.GetInt("AbsentDays") ?? existing?.AbsentDays,
                    LeaveDays = row.GetInt("LeaveDays") ?? existing?.LeaveDays,
                    HalfDays = row.GetInt("HalfDays") ?? existing?.HalfDays,
                    PaidDate = row.GetDate("PaidDate") ?? existing?.PaidDate,
                    PaymentMode = NullIfEmpty(row.GetValue("PaymentMode")) ?? existing?.PaymentMode,
                    Remarks = NullIfEmpty(row.GetValue("Remarks")) ?? existing?.Remarks
                };

                if (existing != null)
                {
                    salary.SalaryId = existing.SalaryId;
                    _salaryProcessingRepo.Update(salary);
                    return null;
                }

                var hasBreakdown = salary.PresentDays.HasValue || salary.AbsentDays.HasValue
                    || salary.LeaveDays.HasValue || salary.HalfDays.HasValue;

                if (hasBreakdown)
                    _salaryProcessingRepo.InsertWithBreakdown(salary);
                else
                    _salaryProcessingRepo.Insert(salary);

                return null;
            });

        #endregion

        #region Helpers

        private int ResolveStaffId(Dictionary<string, string> row)
        {
            var staffId = row.GetInt("StaffId");
            if (staffId.HasValue && staffId > 0)
                return staffId.Value;

            var staffCode = row.GetValue("StaffCode", "EmpCode");
            if (!string.IsNullOrWhiteSpace(staffCode))
            {
                var byCode = _staffRepo.GetByStaffCode(staffCode);
                if (byCode != null)
                    return byCode.StaffId;
            }

            var staffName = row.GetValue("StaffName", "Name");
            if (string.IsNullOrWhiteSpace(staffName))
                return 0;

            var designation = NullIfEmpty(row.GetValue("Designation"));
            var staffList = _staffRepo.GetAll()
                .Where(s => s.StaffName.Equals(staffName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!string.IsNullOrWhiteSpace(designation))
            {
                var byDesignation = staffList
                    .FirstOrDefault(s => (s.Designation ?? "").Equals(designation, StringComparison.OrdinalIgnoreCase));
                if (byDesignation != null)
                    return byDesignation.StaffId;
            }

            if (staffList.Count == 1)
                return staffList[0].StaffId;

            return 0;
        }

        private int ResolveLeadSourceId(Dictionary<string, string> row)
        {
            var sourceId = row.GetInt("LeadSourceId", "SourceId");
            if (sourceId.HasValue && sourceId > 0)
                return sourceId.Value;

            var sourceName = row.GetValue("LeadSourceName", "SourceName");
            if (!string.IsNullOrWhiteSpace(sourceName))
            {
                var match = _leadSourceRepo.GetAll(null)
                    .FirstOrDefault(s => s.SourceName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match.LeadSourceId;
            }

            return _leadSourceRepo.GetAll(null).FirstOrDefault(s => s.IsActive)?.LeadSourceId ?? 0;
        }

        private static CsvImportResult RunImport(
            Stream stream,
            Func<Dictionary<string, string>, int, string?> tryImport)
        {
            var rows = CsvHelper.Parse(stream);
            var result = new CsvImportResult();
            int line = 1;

            foreach (var row in rows)
            {
                line++;
                if (row.Values.All(string.IsNullOrWhiteSpace))
                    continue;

                try
                {
                    var error = tryImport(row, line);
                    if (error != null)
                    {
                        result.FailedCount++;
                        AddError(result, error);
                    }
                    else
                    {
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    AddError(result, $"Row {line}: {ex.Message}");
                }
            }

            return result;
        }

        private static void AddError(CsvImportResult result, string message)
        {
            if (result.Errors.Count < 5)
                result.Errors.Add(message);
        }

        private static CsvFileData MakeFile(
            string fileName,
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<string?>> rows)
        {
            FileContentResult file = CsvHelper.ToFileResult(fileName, headers, rows);
            return new CsvFileData
            {
                Content = file.FileContents!,
                FileName = file.FileDownloadName!
            };
        }

        private static string? Q(IQueryCollection query, string key) =>
            query.TryGetValue(key, out var value) ? value.ToString() : null;

        private static DateTime? QDate(IQueryCollection query, string key)
        {
            var v = Q(query, key);
            if (string.IsNullOrWhiteSpace(v))
                return null;
            return DateTime.TryParse(v, out var d) ? d : null;
        }

        private static int? QInt(IQueryCollection query, string key)
        {
            var v = Q(query, key);
            if (string.IsNullOrWhiteSpace(v))
                return null;
            return int.TryParse(v, out var n) ? n : null;
        }

        private static bool? QBool(IQueryCollection query, string key)
        {
            var v = Q(query, key);
            if (string.IsNullOrWhiteSpace(v))
                return null;
            if (bool.TryParse(v, out var b))
                return b;
            return v switch
            {
                "1" => true,
                "0" => false,
                _ => null
            };
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? S(object? value) => value switch
        {
            null => "",
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            decimal dec => dec.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bool b => b ? "Yes" : "No",
            _ => value.ToString()
        };

        private static string? D(DateTime? value) =>
            value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "";

        private static string? T(TimeSpan? value) =>
            value.HasValue ? value.Value.ToString(@"hh\:mm") : "";

        private static string? B(bool value) => value ? "Yes" : "No";

        private static string?[] BlankRow(int columns)
        {
            var row = new string?[columns];
            for (int i = 0; i < columns; i++)
                row[i] = "";
            return row;
        }

        #endregion
    }
}
