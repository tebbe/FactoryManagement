using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FactoryManagement.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class FactoryManagementEntities : IdentityDbContext<ApplicationUser>
    {
        public FactoryManagementEntities()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static FactoryManagementEntities Create()
        {
            return new FactoryManagementEntities();
        }

        public virtual DbSet<AccNameAssignedWithUser> AccNameAssignedWithUsers { get; set; }
        public virtual DbSet<AccountName> AccountNames { get; set; }
        public virtual DbSet<AccountType> AccountTypes { get; set; }
        public virtual DbSet<Acquisition> Acquisitions { get; set; }
        public virtual DbSet<Acquisition_List> Acquisition_List { get; set; }
        public virtual DbSet<AcquisitionImage> AcquisitionImages { get; set; }
        public virtual DbSet<ActivationCode> ActivationCodes { get; set; }
        public virtual DbSet<All_App_List> All_App_List { get; set; }
        public virtual DbSet<AllSalaryPayList> AllSalaryPayLists { get; set; }
        public virtual DbSet<AllUserLoanList> AllUserLoanLists { get; set; }
        public virtual DbSet<App_User_List> App_User_List { get; set; }
        public virtual DbSet<AssignStoreWithSite> AssignStoreWithSites { get; set; }
        public virtual DbSet<AssignStoreWithUser> AssignStoreWithUsers { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<Attendance_BCR> Attendance_BCR { get; set; }
        public virtual DbSet<AuditInformation> AuditInformations { get; set; }
        public virtual DbSet<BankAccountList> BankAccountLists { get; set; }
        public virtual DbSet<BankBranchList> BankBranchLists { get; set; }
        public virtual DbSet<BankList> BankLists { get; set; }
        public virtual DbSet<BrandList> BrandLists { get; set; }
        public virtual DbSet<Business_Order_FileLists> Business_Order_FileLists { get; set; }
        public virtual DbSet<BusinessOrderList> BusinessOrderLists { get; set; }
        public virtual DbSet<BusinessOrderOtherCost> BusinessOrderOtherCosts { get; set; }
        public virtual DbSet<BusinessOrderProductList> BusinessOrderProductLists { get; set; }
        public virtual DbSet<BusinessOrderSupplierList> BusinessOrderSupplierLists { get; set; }
        public virtual DbSet<BusiOrder_ManualSpec> BusiOrder_ManualSpec { get; set; }
        public virtual DbSet<BusiReceivedItemAssignList> BusiReceivedItemAssignLists { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ChatHistory> ChatHistories { get; set; }
        public virtual DbSet<Client_User_ContactList> Client_User_ContactList { get; set; }
        public virtual DbSet<Client_User_Lists> Client_User_Lists { get; set; }
        public virtual DbSet<Client_Vehicle_Lists> Client_Vehicle_Lists { get; set; }
        public virtual DbSet<ClientContactList> ClientContactLists { get; set; }
        public virtual DbSet<ClientList> ClientLists { get; set; }
        public virtual DbSet<CommentReportCategory> CommentReportCategories { get; set; }
        public virtual DbSet<CountryList> CountryLists { get; set; }
        public virtual DbSet<CurrencyList> CurrencyLists { get; set; }
        public virtual DbSet<Daily_Attendance> Daily_Attendance { get; set; }
        public virtual DbSet<Daily_Raw_Cotton_Report_List> Daily_Raw_Cotton_Report_List { get; set; }
        public virtual DbSet<DepartmentList> DepartmentLists { get; set; }
        public virtual DbSet<Designation> Designations { get; set; }
        public virtual DbSet<DispatchedItemAssignUser> DispatchedItemAssignUsers { get; set; }
        public virtual DbSet<DispatchedItemList> DispatchedItemLists { get; set; }
        public virtual DbSet<DispatchedPartsReturnList> DispatchedPartsReturnLists { get; set; }
        public virtual DbSet<DivisionList> DivisionLists { get; set; }
        public virtual DbSet<DuePaymentRecord> DuePaymentRecords { get; set; }
        public virtual DbSet<EmployeeLeaveRecord> EmployeeLeaveRecords { get; set; }
        public virtual DbSet<EmpPerFormanceReview_LikeUnLike> EmpPerFormanceReview_LikeUnLike { get; set; }
        public virtual DbSet<EmpSalaryPaymentDetail> EmpSalaryPaymentDetails { get; set; }
        public virtual DbSet<ExtraServiceList> ExtraServiceLists { get; set; }
        public virtual DbSet<Factory_Finished_Pro_Title> Factory_Finished_Pro_Title { get; set; }
        public virtual DbSet<FactoryInformation> FactoryInformations { get; set; }
        public virtual DbSet<Finished_Pro_Report_List> Finished_Pro_Report_List { get; set; }
        public virtual DbSet<Finished_Product_List> Finished_Product_List { get; set; }
        public virtual DbSet<HolidayList> HolidayLists { get; set; }
        public virtual DbSet<HolidayPackageList> HolidayPackageLists { get; set; }
        public virtual DbSet<Import_History> Import_History { get; set; }
        public virtual DbSet<Import_Item_Location> Import_Item_Location { get; set; }
        public virtual DbSet<Inventory_Image_Lists> Inventory_Image_Lists { get; set; }
        public virtual DbSet<Inventory_Item_Location> Inventory_Item_Location { get; set; }
        public virtual DbSet<Inventory_Minimum_Finish_List> Inventory_Minimum_Finish_List { get; set; }
        public virtual DbSet<Inventory_Monthly_Stock> Inventory_Monthly_Stock { get; set; }
        public virtual DbSet<Inventory_Pro_Waste_List> Inventory_Pro_Waste_List { get; set; }
        public virtual DbSet<Inventory_Reserved_Item_Lists> Inventory_Reserved_Item_Lists { get; set; }
        public virtual DbSet<InventoryList> InventoryLists { get; set; }
        public virtual DbSet<Leave_Application_List> Leave_Application_List { get; set; }
        public virtual DbSet<LettersCount> LettersCounts { get; set; }
        public virtual DbSet<LineInfo> LineInfoes { get; set; }
        public virtual DbSet<LineMachineList> LineMachineLists { get; set; }
        public virtual DbSet<MachineList> MachineLists { get; set; }
        public virtual DbSet<MachineMaintenanceLog> MachineMaintenanceLogs { get; set; }
        public virtual DbSet<MachinePartsList> MachinePartsLists { get; set; }
        public virtual DbSet<MachinePartsNotInstalledList> MachinePartsNotInstalledLists { get; set; }
        public virtual DbSet<Machine> Machines { get; set; }
        public virtual DbSet<MachineTypeName> MachineTypeNames { get; set; }
        public virtual DbSet<ManualIndent> ManualIndents { get; set; }
        public virtual DbSet<ManualIndentReceiveTransaction> ManualIndentReceiveTransactions { get; set; }
        public virtual DbSet<ManualIndentVoucher> ManualIndentVouchers { get; set; }
        public virtual DbSet<Multiple_HolidayLists> Multiple_HolidayLists { get; set; }
        public virtual DbSet<NameTitle> NameTitles { get; set; }
        public virtual DbSet<OrderStatusList> OrderStatusLists { get; set; }
        public virtual DbSet<Payment_Term_Type> Payment_Term_Type { get; set; }
        public virtual DbSet<ProductItemList> ProductItemLists { get; set; }
        public virtual DbSet<ProductTypeList> ProductTypeLists { get; set; }
        public virtual DbSet<ProductUnitList> ProductUnitLists { get; set; }
        public virtual DbSet<ReligionList> ReligionLists { get; set; }
        public virtual DbSet<ReportedComment> ReportedComments { get; set; }
        public virtual DbSet<RoleNameList> RoleNameLists { get; set; }
        public virtual DbSet<RolePermissionList> RolePermissionLists { get; set; }
        public virtual DbSet<Salary_Acc_Pay_MonthList> Salary_Acc_Pay_MonthList { get; set; }
        public virtual DbSet<Salary_Account_Type> Salary_Account_Type { get; set; }
        public virtual DbSet<Salary_Package_Account_Lists> Salary_Package_Account_Lists { get; set; }
        public virtual DbSet<Salary_Package_Catagory> Salary_Package_Catagory { get; set; }
        public virtual DbSet<Salary_Package_List> Salary_Package_List { get; set; }
        public virtual DbSet<SalaryPaymentPckDetail> SalaryPaymentPckDetails { get; set; }
        public virtual DbSet<SickLeaveType> SickLeaveTypes { get; set; }
        public virtual DbSet<Site_Unit_Lists> Site_Unit_Lists { get; set; }
        public virtual DbSet<SiteList> SiteLists { get; set; }
        public virtual DbSet<SiteWarehouseList> SiteWarehouseLists { get; set; }
        public virtual DbSet<SparePartsList> SparePartsLists { get; set; }
        public virtual DbSet<StockItemList> StockItemLists { get; set; }
        public virtual DbSet<StoreItemDispatchedHistory> StoreItemDispatchedHistories { get; set; }
        public virtual DbSet<StoreItemList> StoreItemLists { get; set; }
        public virtual DbSet<StoreList> StoreLists { get; set; }
        public virtual DbSet<TmpInventoryOrderList> TmpInventoryOrderLists { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionEditReason> TransactionEditReasons { get; set; }
        public virtual DbSet<TransactionsEditCount> TransactionsEditCounts { get; set; }
        public virtual DbSet<TransactionType> TransactionTypes { get; set; }
        public virtual DbSet<UnitList> UnitLists { get; set; }
        public virtual DbSet<User_Bank_Account> User_Bank_Account { get; set; }
        public virtual DbSet<User_Rating_List> User_Rating_List { get; set; }
        public virtual DbSet<User_Uploaded_Document> User_Uploaded_Document { get; set; }
        public virtual DbSet<UserInformation> UserInformations { get; set; }
        public virtual DbSet<UserItemDispatchLoc> UserItemDispatchLocs { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<UserPerformanceReview> UserPerformanceReviews { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<Voucher_Other_Cost_Type> Voucher_Other_Cost_Type { get; set; }
        public virtual DbSet<Voucher_Other_Costs_List> Voucher_Other_Costs_List { get; set; }
        public virtual DbSet<Voucher_Service_List> Voucher_Service_List { get; set; }
        public virtual DbSet<Voucher_Transaction_List> Voucher_Transaction_List { get; set; }
        public virtual DbSet<VoucherType> VoucherTypes { get; set; }
        public virtual DbSet<WarehouseList> WarehouseLists { get; set; }
        public virtual DbSet<WorkingDayList> WorkingDayLists { get; set; }
        public virtual DbSet<WorkingScheduleList> WorkingScheduleLists { get; set; }
        public virtual DbSet<View_Account_Name> View_Account_Name { get; set; }
        public virtual DbSet<View_All_AcquisitionList> View_All_AcquisitionList { get; set; }
        public virtual DbSet<View_All_Approved_AcquisitionList> View_All_Approved_AcquisitionList { get; set; }
        public virtual DbSet<View_All_Inven_ForCart> View_All_Inven_ForCart { get; set; }
        public virtual DbSet<View_All_InventoryList> View_All_InventoryList { get; set; }
        public virtual DbSet<View_All_Loan_History> View_All_Loan_History { get; set; }
        public virtual DbSet<View_All_Revert_Trans> View_All_Revert_Trans { get; set; }
        public virtual DbSet<View_All_Salary_Pay_His> View_All_Salary_Pay_His { get; set; }
        public virtual DbSet<View_All_StoreLists> View_All_StoreLists { get; set; }
        public virtual DbSet<View_All_StoreWarehouseLists> View_All_StoreWarehouseLists { get; set; }
        public virtual DbSet<View_All_TransactionEdit> View_All_TransactionEdit { get; set; }
        public virtual DbSet<View_All_WarehouseLists> View_All_WarehouseLists { get; set; }
        public virtual DbSet<View_AllAttendance> View_AllAttendance { get; set; }
        public virtual DbSet<View_Attendance_BCR> View_Attendance_BCR { get; set; }
        public virtual DbSet<View_Audit_Information> View_Audit_Information { get; set; }
        public virtual DbSet<View_Bank_info> View_Bank_info { get; set; }
        public virtual DbSet<View_Branch_info> View_Branch_info { get; set; }
        public virtual DbSet<View_BrandList> View_BrandList { get; set; }
        public virtual DbSet<View_Busi_Details_ForClient> View_Busi_Details_ForClient { get; set; }
        public virtual DbSet<View_Busi_Order_Assigned_Item> View_Busi_Order_Assigned_Item { get; set; }
        public virtual DbSet<View_BusinessOrder_File> View_BusinessOrder_File { get; set; }
        public virtual DbSet<View_Categories> View_Categories { get; set; }
        public virtual DbSet<View_Chat_History> View_Chat_History { get; set; }
        public virtual DbSet<View_Client_Info> View_Client_Info { get; set; }
        public virtual DbSet<View_Daily_Attendance> View_Daily_Attendance { get; set; }
        public virtual DbSet<View_Dep_Details> View_Dep_Details { get; set; }
        public virtual DbSet<View_DisItemReturnDetails> View_DisItemReturnDetails { get; set; }
        public virtual DbSet<View_Dispacth_Item_Details> View_Dispacth_Item_Details { get; set; }
        public virtual DbSet<View_Finished_Product_List> View_Finished_Product_List { get; set; }
        public virtual DbSet<View_Holiday_Lists> View_Holiday_Lists { get; set; }
        public virtual DbSet<View_HolidayPackage> View_HolidayPackage { get; set; }
        public virtual DbSet<View_Import_History_Details> View_Import_History_Details { get; set; }
        public virtual DbSet<View_Import_Item_Details> View_Import_Item_Details { get; set; }
        public virtual DbSet<View_Indent_Transaction> View_Indent_Transaction { get; set; }
        public virtual DbSet<View_Inven_Dis_Voucher_List> View_Inven_Dis_Voucher_List { get; set; }
        public virtual DbSet<View_Inven_Item_Loc_Details> View_Inven_Item_Loc_Details { get; set; }
        public virtual DbSet<View_Inventory_Finish_Item_List> View_Inventory_Finish_Item_List { get; set; }
        public virtual DbSet<View_Inventory_Item_Details> View_Inventory_Item_Details { get; set; }
        public virtual DbSet<View_Inventory_TmpOrderList> View_Inventory_TmpOrderList { get; set; }
        public virtual DbSet<View_Invoice_Service> View_Invoice_Service { get; set; }
        public virtual DbSet<View_Leave_Application> View_Leave_Application { get; set; }
        public virtual DbSet<View_Line_Machine_Info> View_Line_Machine_Info { get; set; }
        public virtual DbSet<View_Machine_Line_Dept_Unit_List> View_Machine_Line_Dept_Unit_List { get; set; }
        public virtual DbSet<View_Machine_Lists> View_Machine_Lists { get; set; }
        public virtual DbSet<View_Machine_Parts> View_Machine_Parts { get; set; }
        public virtual DbSet<View_Machine_Type_Name> View_Machine_Type_Name { get; set; }
        public virtual DbSet<View_Maintenance_Log> View_Maintenance_Log { get; set; }
        public virtual DbSet<View_Manual_Indent> View_Manual_Indent { get; set; }
        public virtual DbSet<View_Manual_Indent_Voucher> View_Manual_Indent_Voucher { get; set; }
        public virtual DbSet<View_Product_Type_Lists> View_Product_Type_Lists { get; set; }
        public virtual DbSet<View_ProList_WithLC> View_ProList_WithLC { get; set; }
        public virtual DbSet<View_Routedhistory> View_Routedhistory { get; set; }
        public virtual DbSet<View_SiteLists> View_SiteLists { get; set; }
        public virtual DbSet<View_Sup_Pro_Lists> View_Sup_Pro_Lists { get; set; }
        public virtual DbSet<View_Today_Leave_User_List> View_Today_Leave_User_List { get; set; }
        public virtual DbSet<View_Transaction> View_Transaction { get; set; }
        public virtual DbSet<View_UnitList> View_UnitList { get; set; }
        public virtual DbSet<View_User_Att_Input> View_User_Att_Input { get; set; }
        public virtual DbSet<View_User_Review_List> View_User_Review_List { get; set; }
        public virtual DbSet<View_User_Salary_Details> View_User_Salary_Details { get; set; }
        public virtual DbSet<View_User_Uploaded_File_List> View_User_Uploaded_File_List { get; set; }
        public virtual DbSet<View_UserLists> View_UserLists { get; set; }
        public virtual DbSet<View_Voucher_Transaction> View_Voucher_Transaction { get; set; }
        public virtual DbSet<View_VoucherInfo> View_VoucherInfo { get; set; }
        public virtual DbSet<View_VoucherServicesCostList> View_VoucherServicesCostList { get; set; }

        [DbFunction("FactoryManagementEntities", "GetUserPerformanceReview")]
        public virtual IQueryable<GetUserPerformanceReview_Result> GetUserPerformanceReview(Nullable<long> userId, Nullable<long> loginUserId)
        {
            var userIdParameter = userId.HasValue ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(long));

            var loginUserIdParameter = loginUserId.HasValue ?
                new ObjectParameter("LoginUserId", loginUserId) :
                new ObjectParameter("LoginUserId", typeof(long));

            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<GetUserPerformanceReview_Result>("[FactoryManagementEntities].[GetUserPerformanceReview](@UserId, @LoginUserId)", userIdParameter, loginUserIdParameter);
        }
    }
}