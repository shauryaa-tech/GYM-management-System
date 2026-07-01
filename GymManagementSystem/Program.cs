using GymManagement.Data;
using GymManagement.Data.Repositories;
using GymManagement.Data.Repositories.Interfaces;
using GymManagement.Helpers;
using GymManagement.Repositories;
using GymManagement.Services;
using GymManagement.Services.Interfaces;
using GymManagement.Services.PaymentProviders.Implementations;
using GymManagement.Services.Paytm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionAuthorizationFilter>();
})
    .AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.Configure<PaytmSettings>(
    builder.Configuration.GetSection(PaytmSettings.SectionName));
builder.Services.Configure<PhonePeSettings>(
    builder.Configuration.GetSection(PhonePeSettings.SectionName));
builder.Services.Configure<RazorpaySettings>(
    builder.Configuration.GetSection(RazorpaySettings.SectionName));
builder.Services.Configure<CashfreeSettings>(
    builder.Configuration.GetSection(CashfreeSettings.SectionName));
builder.Services.Configure<GymManagement.Services.WhatsApp.WhatsAppSettings>(
    builder.Configuration.GetSection(GymManagement.Services.WhatsApp.WhatsAppSettings.SectionName));

builder.Services.AddHttpClient("Paytm", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("PhonePe", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("Razorpay", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("Cashfree", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("WhatsApp", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Db Helper
builder.Services.AddScoped<DbHelper>();
builder.Services.AddScoped<DatabaseSchemaUpdater>();

// Repositories
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<MembershipRepository>();
builder.Services.AddScoped<MemberRepository>();
builder.Services.AddScoped<MembersRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<StaffRepository>();
builder.Services.AddScoped<ExerciseRepository>();
builder.Services.AddScoped<DietRepository>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<ClassBookingRepository>();
builder.Services.AddScoped<EquipmentRepository>();
builder.Services.AddScoped<VendorRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<StockPurchaseRepository>();
builder.Services.AddScoped<StockIssueRepository>();
builder.Services.AddScoped<EquipmentMaintenanceRepository>();
builder.Services.AddScoped<ExpenseHeadRepository>();
builder.Services.AddScoped<ExpenseRepository>();
builder.Services.AddScoped<StaffAttendanceRepository>();
builder.Services.AddScoped<SalaryProcessingRepository>();
builder.Services.AddScoped<LeadSourceRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<LeadRepository>();
builder.Services.AddScoped<MembershipTransactionRepository>();
builder.Services.AddScoped<WorkoutPlanRepository>();
builder.Services.AddScoped<DietPlanRepository>();
builder.Services.AddScoped<PTSessionRepository>();
builder.Services.AddScoped<TrainerAssignmentRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<NavbarSearchRepository>();
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<WhatsAppBotSessionRepository>();
builder.Services.AddScoped<WhatsAppApiSetupRepository>();
builder.Services.AddScoped<GymManagement.Services.WhatsApp.IWhatsAppSettingsProvider, GymManagement.Services.WhatsApp.WhatsAppSettingsProvider>();
builder.Services.AddScoped<IWhatsAppApiSetupService, GymManagement.Services.WhatsApp.WhatsAppApiSetupService>();

builder.Services.AddScoped<GymManagement.Services.WhatsApp.IWhatsAppService, GymManagement.Services.WhatsApp.WhatsAppService>();
builder.Services.AddScoped<GymManagement.Services.WhatsApp.IWhatsAppBotService, GymManagement.Services.WhatsApp.WhatsAppBotService>();

builder.Services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IPaytmService, PaytmService>();
builder.Services.AddScoped<GymManagement.Services.PaymentProviders.IPaymentProvider, GymManagement.Services.PaymentProviders.Implementations.PaytmPaymentProvider>();
builder.Services.AddScoped<GymManagement.Services.PaymentProviders.IPaymentProvider, GymManagement.Services.PaymentProviders.Implementations.PhonePePaymentProvider>();
builder.Services.AddScoped<GymManagement.Services.PaymentProviders.IPaymentProvider, GymManagement.Services.PaymentProviders.Implementations.RazorpayPaymentProvider>();
builder.Services.AddScoped<GymManagement.Services.PaymentProviders.IPaymentProvider, GymManagement.Services.PaymentProviders.Implementations.CashfreePaymentProvider>();
builder.Services.AddScoped<GymManagement.Services.PaymentProviders.PaymentProviderFactory>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

builder.Services.AddScoped<SalaryRuleMasterRepository>();
builder.Services.AddScoped<SalaryCalculationService>();
builder.Services.AddScoped<SalaryStatementService>();
builder.Services.AddScoped<BankStatementService>();

builder.Services.AddScoped<ITrainerAutoAssignService, TrainerAutoAssignService>();

builder.Services.AddScoped<GymManagement.Services.Interfaces.IModuleCsvService, GymManagement.Services.ModuleCsvService>();

builder.Services.AddScoped<PermissionRepository>();

builder.Services.AddScoped<RolePermissionRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseSchemaUpdater>().ApplyPendingMigrations();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();