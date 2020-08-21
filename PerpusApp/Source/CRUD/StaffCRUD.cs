using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.CRUD
{
    public sealed class StaffCRUD
    {
        public static List<UserStaff> ReadAll(string connStr)
        {
            List<UserStaff> stf = new List<UserStaff>();
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr = "SELECT * FROM `db_perpus`.`staff` WHERE (`stf_rec_status` = '1');";
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            while (_data.Read())
            {
                stf.Add(new UserStaff {
                    stf_id = _data.GetInt32(0),
                    stf_u_id = _data.GetInt32(1),
                    stf_fullname = _data.GetString(2),
                    stf_email = _data.GetString(3),
                    stf_contact = _data.GetString(4),
                    stf_address = _data.GetString(5),
                    stf_shift = _data.GetDateTime(6).ToString("dd/MM/yyyy HH:mm:ss"),
                    stf_status = _data.GetInt16(7)
                });
            }
            _conn.Close();
            return stf;
        }
    }
}