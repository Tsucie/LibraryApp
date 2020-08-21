using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PerpusApp.Source.CRUD;
using PerpusApp.Source.Models;
using PerpusApp.Source.General;

namespace PerpusApp.Source.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffCategoryController : ControllerBase
    {
        [HttpGet("GetList")]
        public async Task<IActionResult> ReadAllData()
        {
            try
            {
                List<StaffCategory> staffCategories = await Task.Run(() => StaffCategoryCRUD.ReadAll(Startup.db_perpus_ConnStr));
                return Ok(staffCategories);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetData/{sc_id}")]
        public ActionResult ReadData([FromRoute] int sc_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                StaffCategory sc = StaffCategoryCRUD.Read(Startup.db_perpus_ConnStr, sc_id);
                if(sc.Equals(null)) throw new Exception("", new Exception("Error Get data from Database!"));

                return Ok(sc);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPost("Add")]
        public ActionResult CreateData([FromBody] StaffCategory sc)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(sc.sc_name)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(StaffCategoryCRUD.Create(Startup.db_perpus_ConnStr, sc) != 1) throw new Exception("", new Exception("Data is not added in database"));

                response.Code = 1;
                response.Message = "Kategori staff berhasil ditambahkan!";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPut("Edit")]
        public ActionResult UpdateData([FromBody] StaffCategory sc)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(sc.sc_name)) throw new Exception("", new Exception("Data is not updated. Incomplete data"));

                if(StaffCategoryCRUD.Update(Startup.db_perpus_ConnStr, (int)sc.sc_id, sc) != 1) throw new Exception("", new Exception("Data is not Updated in database"));

                response.Code = 1;
                response.Message = "Kategori staff berhasil di Update!";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpDelete("Delete")]
        public ActionResult DeleteData([FromBody] StaffCategory sc)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if (sc.sc_id.Equals(null)) throw new Exception("", new Exception("Data is not deleted. Incomplete data"));
                
                if (StaffCategoryCRUD.Delete(Startup.db_perpus_ConnStr, (int)sc.sc_id) != 1) throw new Exception("", new Exception("Data is not deleted in database"));

                response.Code = 1;
                response.Message = "Kategori staff berhasil dihapus!";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }
    }
}