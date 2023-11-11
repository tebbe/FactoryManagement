using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory
{
    public class InventoryListModelView
    {
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Please enter product name")]
        [Remote("ProductNameExist", "RemoteValidation", AdditionalFields = "InventoryId", ErrorMessage = "Product name already exsits")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Please select one")]
        public int ProductTypeId { get; set; }
        public string LocationWithLc { get; set; }
        public string ProductTypeName { get; set; }
        public string Brand { get; set; }
        public string ModelName { get; set; }
        public string Model { get; set; }
        public string Rack { get; set; }
        [Required(ErrorMessage = "Please enter quantity")]
        [RegularExpression(@"^[0-9]\d*$", ErrorMessage = "Only Be A Positive Number")]
        //public int Quantity { get; set; }
        public Nullable<double> Quantity { get; set; }

        public Nullable<double> Min_Quantity { get; set; }

        [RegularExpression(@"^0*[1-9][0-9]*(\.[0-9]+)?|0+\.[0-9]*[1-9][0-9]*$", ErrorMessage = "Price must be a number")]
        public Nullable<decimal> Price { get; set; }

        [Required(ErrorMessage = "Please select unit")]
        public int UnitId { get; set; }
        public string Withunit { get; set; }
       

        public bool CanbeOrdered { get; set; }
        public bool CanbeSold { get; set; }
        public bool CanbeRepaired { get; set; }
        public bool CanbePerishable { get; set; }
        public bool CanbeReplaceable { get; set; }
        public bool CanbeTopUp { get; set; }
        public bool CanbeBreakable { get; set; }


        public bool IsDefineLocation { get; set; }


        public Nullable<int> Warranty { get; set; }
        public Nullable<int> WarrantyDay { get; set; }
        public int WarrantyMonth { get; set; }
        public int WarrantyYear { get; set; }
        public string InternalReferenceNo { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public DateTime ExpDateLeftTotal { get; set; }
        public Nullable<System.DateTime> ExpiredDate { get; set; }
        public string ImageOriginalName { get; set; }
        public string ProductImageName { get; set; }
        public string ImageRename { get; set; }
        public List<SelectListItem> ListItems { get; set; }
        public string Edit { get; set; }



        //********************* Tree View ***************************
        public int TotalProductCount { get; set; }
        public int ViewTypeId { get; set; }
        public int TotalCount { get; set; }

        //************** Search Bar **************
        public string TextForSearch { get; set; }

        //********* Temporary Order *****
        public string Tempdata { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public string ItemNameWithPrice { get; set; }
        public Nullable<decimal> TotalCost { get; set; }
        public long OrderedBy { get; set; }
        public int OrderedQuantity { get; set; }

        public string CurrencySymbol { get; set; }
        public bool IsNewImage { get; set; }
        public string UnitName { get; set; }
        //public bool HasSubUnit { get; set; }
        public Nullable<int> SubQuantity { get; set; }
        public Nullable<int> SubUnits { get; set; }
        public string SubUnitName { get; set; }

        public string WarrantyFor { get; set; }
        public bool IsWarrantyStart { get; set; }




       // [Required(ErrorMessage = "Please select a Country")]
        public string Country { get; set; }
        public string CountryName { get; set; }
        public Nullable<int> BrandId { get; set; }

        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> NewExpiredDate { get; set; }
        public Nullable<long> UpatedBy { get; set; }

        public Nullable<System.DateTime> UpdatedDate { get; set; }

        public Nullable<int> Vendor { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<double> ReservedQuantity { get; set; }
        public Nullable<double> Available_Quantity { get; set; }

        public int Quan_Cart { get; set; }
        public long Cart_AddedBy { get; set; }

        public Nullable<int> lcno { get; set; }
        public string Recv_Quan { get; set; }

        public int InMultipleLocation { get; set; }
        public Nullable<int> LocationId { get; set; }
        public string SelectedLocId { get; set; }


        public Nullable<long> ImportId { get; set; }
        public string LC { get; set; }
        public string ProNameWithLc { get; set; }
    }
    public class ChildClassForSubcategory
    {
        public string text { get; set; }
        public string id { get; set; }
        public int ProductTypeId { get; set; }
        public int Count { get; set; }

    }
    public class InventoryOrderViewModel
    {
        public int InventoryOrderId { get; set; }
        [Required(ErrorMessage = "Product Name is invalid")]
        public int InventoryId { get; set; }
        [Required(ErrorMessage = "Product Name is required")]
        public string ProductName { get; set; }


        [Required(ErrorMessage = "Quantity is required")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Only Be A Positive Number")]
        public int Quantity { get; set; }
        public string withunit { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public int ForPersonalUse { get; set; }
        public int ForWorkUse { get; set; }
        public Nullable<int> RequiredAsd { get; set; }
        public bool checkboxForRequiredAsd { get; set; }
        public Nullable<System.DateTime> RequiredDate { get; set; }
        public string OrderNote { get; set; }
        public System.DateTime OrderedDate { get; set; }
        public long OrederedBy { get; set; }

        [Required(ErrorMessage = "Estimate Price is required")]
        [RegularExpression(@"^0*[0-9][0-9]*(\.[0-9]+)?|0+\.[0-9]*[1-9][0-9]*$", ErrorMessage = "Price must be a number")]
        public decimal EstimatePrice { get; set; }
        public string OrderFor { get; set; }
        public int CollectionType { get; set; }
        public string ChkCollectionType { get; set; }
        public string BtnName { get; set; }
        public List<SelectListItem> TempListItems { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Status { get; set; }
        public int CartType { get; set; }
        public string CurrencySymbol { get; set; }
        //custom order
        public int CustomOrderId { get; set; }
        public string ProductLink { get; set; }
        public string ProductImageName { get; set; }
        public string ProductImageOriginalName { get; set; }
        public string customEdit { get; set; }

        //UserInformation
        public long UserCode { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string OrderedName { get; set; }
        public string Department { get; set; }
        public string Picture { get; set; }

        public int OrderType { get; set; }
        public int OrderStatus { get; set; }
    }
    public class InventoryPurchaseView
    {
        public int InventoryOrderId { get; set; }
        public string PurchaseType { get; set; }
        public string UserId { get; set; }
        public long UserCode { get; set; }
        public string AssignUser { get; set; }
        public string Picture { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<System.DateTime> BuyDate { get; set; }
        public string OrderReceiptName { get; set; }
        public string GetUserlist { get; set; }
        public string BtnName { get; set; }
        public string CurrencySymbol { get; set; }
        //***view purchase order***//
        public int OrderType { get; set; }
        public int InventoryId { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Withunit { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }

        [Required(ErrorMessage = "Estimate Price is required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid price")]
        public decimal EstimatePrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid amount")]
        public Nullable<decimal> ConvienceCharge { get; set; }


        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid amount")]
        public Nullable<decimal> Expense { get; set; }

        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid amount")]
        public Nullable<decimal> DeliveryCharge { get; set; }
        public string OrderedName { get; set; }
        public Nullable<System.DateTime> RequiredDate { get; set; }
        public Nullable<System.DateTime> ConfirmedDate { get; set; }
        public int ForPersonalUse { get; set; }
        public int ForWorkUse { get; set; }
        public int CollectionType { get; set; }
        public string Department { get; set; }
        public System.DateTime OrderedDate { get; set; }
        public long OrderedBy { get; set; }
        public int Status { get; set; }
        public int CustomOrderId { get; set; }
        public string ProductLink { get; set; }
        public int ForOfficeUse { get; set; }
        public Nullable<int> RequiredAsd { get; set; }
        public string OrderNote { get; set; }
        public string ProductImageName { get; set; }
        ////*** Complaint***///
        public int PurchaseOrderId { get; set; }
        public string PurchasedBy { get; set; }
        public string ComplaintType { get; set; }

        [Required(ErrorMessage = "Write Description Here")]
        public string Complaint { get; set; }
        public Nullable<System.DateTime> PurchaseDate { get; set; }
        public string nameWithpic { get; set; }
        public string itemStatus { get; set; }

    }
    public class PurchaseComplaintView
    {
        public int InventoryOrderId { get; set; }
        public int PurchaseOrderId { get; set; }
        public string PurchasedBy { get; set; }
        public string ComplaintType { get; set; }
        public string Complaint { get; set; }
        public Nullable<System.DateTime> PurchaseDate { get; set; }
        public string nameWithpic { get; set; }
        public string Withunit { get; set; }
        public int Status { get; set; }
        public string ProductName { get; set; }
        public string ProductLink { get; set; }
        public string ProductImageName { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        
    }
}