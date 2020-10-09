using IdealExchange.LogsSystemClass;
using IdealExchange.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IdealExchange;
using IdealExchange.Models.Core;
using IdealExchange.Models.Helpers;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Services;
namespace IdealExchange.Controllers
{
    public class CountriesController : BaseController
    {

        [CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries)]
        public ActionResult Index()
        {
            var maxCode = Db.Table_Countries.Select(s => s.CountryCode).DefaultIfEmpty(0).Max() + 1;
             
            return View(new Table_Countries() { CountryCode = maxCode });
        }
        public ActionResult GetEmployees(int? iDisplayLength, int? iDisplayStart, int? iSortCol_0,
           string sSortDir_0, string sSearch)
        {
            int displayLength = iDisplayLength ?? 10;
            int displayStart = iDisplayStart ?? 0;
            int sortCol = iSortCol_0 ?? 0;
            string sortDir = sSortDir_0 ?? "";
            string search = sSearch ?? "";

            string cs = MainConfig.GetConfig().ConnectinString;
            List<Employee> listEmployees = new List<Employee>();
            int filteredCount = 0;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("spGetEmployees", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                SqlParameter paramDisplayLength = new SqlParameter()
                {
                    ParameterName = "@DisplayLength",
                    Value = displayLength
                };
                cmd.Parameters.Add(paramDisplayLength);

                SqlParameter paramDisplayStart = new SqlParameter()
                {
                    ParameterName = "@DisplayStart",
                    Value = displayStart
                };
                cmd.Parameters.Add(paramDisplayStart);

                SqlParameter paramSortCol = new SqlParameter()
                {
                    ParameterName = "@SortCol",
                    Value = sortCol
                };
                cmd.Parameters.Add(paramSortCol);

                SqlParameter paramSortDir = new SqlParameter()
                {
                    ParameterName = "@SortDir",
                    Value = sortDir
                };
                cmd.Parameters.Add(paramSortDir);

                SqlParameter paramSearchString = new SqlParameter()
                {
                    ParameterName = "@Search",
                    Value = string.IsNullOrEmpty(search) ? null : search
                };
                cmd.Parameters.Add(paramSearchString);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Employee employee = new Employee();
                    employee.Id = Convert.ToInt32(rdr["Id"]);
                    filteredCount = Convert.ToInt32(rdr["TotalCount"]);
                    employee.FirstName = rdr["FirstName"].ToString();
                    employee.LastName = rdr["LastName"].ToString();
                    employee.Gender = rdr["Gender"].ToString();
                    employee.JobTitle = rdr["JobTitle"].ToString();
                    listEmployees.Add(employee);
                }
            }

            var result = new
            {
                iTotalRecords = GetEmployeeTotalCount(),
                iTotalDisplayRecords = filteredCount,
                aaData = listEmployees
            };

            JavaScriptSerializer js = new JavaScriptSerializer();
            var  aa  = js.Serialize(result);

            return this.Json(new
            {
                iTotalRecords = GetEmployeeTotalCount(),
                iTotalDisplayRecords = filteredCount,
                aaData = listEmployees
            }, JsonRequestBehavior.AllowGet);
        }

        private int GetEmployeeTotalCount()
        {
            int totalEmployeeCount = 0;
            string cs = MainConfig.GetConfig().ConnectinString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new
                    SqlCommand("select count(*) from tblEmployees", con);
                con.Open();
                totalEmployeeCount = (int)cmd.ExecuteScalar();
            }
            return totalEmployeeCount;
        }


[CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries)]
        public ActionResult _GetCountries()
        {
            return PartialView("_GetCountries", Db.Table_Countries.ToList());
        }

        [CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries, Permissions = Permissions.CanAdd)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCountry(Table_Countries model)
        {
            try
            {
                //var msgs = new List<Msg>();
                var chkIfExits = Db.Table_Countries.SingleOrDefault(s=> s.ArabicName == model.ArabicName || s.CountryCode == model.CountryCode);
                if (chkIfExits == null)
                {
                    Db.Table_Countries.Add(model);
                    await Db.SaveChangesAsync();
                    return Json(new Msg() { Title = Resources.Common.AddCountry, Subject = Resources.Common.AddedDoneMsg, MsgType = MsgType.Success });
                }
                else
                {
                    return Json(new Msg() { Title = Resources.Common.DuplicateName, Subject = Resources.Common.DuplicateMsg, MsgType = MsgType.Warning });
                }
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                while (ex.InnerException != null) { ex = ex.InnerException; }
                LogsSystem.AddError((int)PagesCodes.MainData.Main.RegisterCountries, ex.Message, line, (int)ActionsTypes.Add,"");
                return Json(new Msg() { Title = "حدث خطأ", Subject = ex.Message, MsgType = MsgType.Error });
            }
        }

        [CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries, Permissions = Permissions.CanEdit)]
        public ActionResult EditCountry(string RowID)
        {
            var DecID = int.Parse(EncriptionAndDecription.Decrypt(RowID));
            var GetRow = Db.Table_Countries.Find(DecID);
            if (GetRow != null)
            {
                ViewBag.RowID = RowID;
                return PartialView("_EditCountry", GetRow);
            }
            return Json(new Msg() { Title = "حدث خطأ", Subject = "", MsgType = MsgType.Error });
        }
        [CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries, Permissions = Permissions.CanEdit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCountry(string RowID, Table_Countries model)
        {
            try
            {
                //var msgs = new List<Msg>();
                var DecRowID = int.Parse(EncriptionAndDecription.Decrypt(RowID));
                var GetRow = Db.Table_Countries.FirstOrDefault(s=>(s.ArabicName == model.ArabicName || s.CountryCode == model.CountryCode) && s.ID != DecRowID);
                if (GetRow != null && GetRow.ID != DecRowID)
                {
                    return Json(new Msg() { Title = Resources.Common.DuplicateName, Subject = Resources.Common.DuplicateMsg, MsgType = MsgType.Warning });
                }
                else
                {
                    GetRow = Db.Table_Countries.Find(DecRowID);
                    GetRow.ArabicName = model.ArabicName;
                    GetRow.EnglishName = model.EnglishName;
                    GetRow.CountryCode = model.CountryCode;
                    await Db.SaveChangesAsync();
                    return Json(new Msg() { Title = Resources.Common.EditCountry, Subject = Resources.Common.EditDoneMsg, MsgType = MsgType.Success });

                }
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                while (ex.InnerException != null) { ex = ex.InnerException; }
                LogsSystem.AddError((int)PagesCodes.MainData.Main.RegisterCountries, ex.Message, line, (int)ActionsTypes.Edit,"");
                return Json(new Msg() { Title = "حدث خطأ", Subject = ex.Message, MsgType = MsgType.Error });
            }
        }

        [CustomAuthorize(PageCode = (int)PagesCodes.MainData.Main.RegisterCountries, Permissions = Permissions.CanDelete)]
        public async Task<ActionResult> DeleteCountry(string RowID)
        {
            var DecID = int.Parse(EncriptionAndDecription.Decrypt(RowID));
            var GetRow = Db.Table_Countries.Find(DecID);
            if (GetRow != null)
            {
                try
                {
                    Db.Table_Countries.Remove(GetRow);
                    await Db.SaveChangesAsync();
                    return Json(new Msg() { Title = Resources.Common.Delete, Subject = Resources.Common.DeleteDoneMsg, MsgType = MsgType.Success }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    while (ex.InnerException != null) { ex = ex.InnerException; }
                    LogsSystem.AddError((int)PagesCodes.MainData.Main.RegisterCountries, ex.Message, line, (int)ActionsTypes.Delete,"");
                    return Json(new Msg() { Title = Resources.Common.Delete, Subject = Resources.Common.DeleteErrorUsing, MsgType = MsgType.Error }, JsonRequestBehavior.AllowGet);
                }
            }
            return Content("");
        }
    }
}