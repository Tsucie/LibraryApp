using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.CRUD
{
    public sealed class StaffCategoryCRUD
    {
        public static List<StaffCategory> ReadAll(string connStr)
        {
            List<StaffCategory> sc = new List<StaffCategory>();
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr = "SELECT * FROM `db_perpus`.`staff_category`;";
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            while (_data.Read())
            {
                sc.Add(new StaffCategory {
                    sc_id = _data.GetInt32(0),
                    sc_name = _data.GetString(1),
                    sc_desc = _data.GetString(2)
                });
            }
            _conn.Close();
            return sc;
        }

        public static StaffCategory Read(string connStr, int sc_id)
        {
            StaffCategory sc = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr = "SELECT * FROM `db_perpus`.`staff_category` WHERE (`sc_id` = '"+sc_id+"');";
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                sc = new StaffCategory();
                sc.sc_id = _data.GetInt32(0);
                sc.sc_name = _data.GetString(1);
                sc.sc_desc = _data.GetString(2);
            }
            _conn.Close();
            return sc;
        }

        public static int Create(string connStr, StaffCategory sc)
        {
            int affectedRow = 0;
            Random rand = new Random();
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "INSERT INTO `db_perpus`.`staff_category` "+
                            "(`sc_id`,`sc_name`,`sc_desc`) "+
                            "VALUES ('"+rand.Next()+"','"+sc.sc_name+"','"+sc.sc_desc+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int Update(string connStr, int sc_id, StaffCategory sc)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "UPDATE `db_perpus`.`staff_category` SET `sc_name` = '"+sc.sc_name+"', `sc_desc` = '"+sc.sc_desc+"' WHERE (`sc_id` = '"+sc_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int Delete(string connStr, int sc_id)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "DELETE FROM `db_perpus`.`staff_category` WHERE (`sc_id` = '"+sc_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }
    }
}