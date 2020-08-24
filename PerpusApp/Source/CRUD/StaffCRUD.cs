using System;
using System.Data;
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
            string sqlStr =
                "SELECT * FROM `db_perpus`.`staff` "+
                "INNER JOIN db_perpus.staff_category ON staff.stf_sc_id = staff_category.sc_id "+
                "INNER JOIN db_perpus.users ON staff.stf_u_id = users.u_id "+
                "LEFT JOIN db_perpus.user_photo ON users.u_id = user_photo.up_u_id "+
                "WHERE (`stf_rec_status` = '1');";
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            DataTable dt = new DataTable();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_cmd);
            dataAdapter.Fill(dt);
            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    UserStaff s = new UserStaff();
                    if(dr["up_id"] != DBNull.Value)
                    {
                        s.up_photo = (byte[])dr["up_photo"];
                        s.up_filename = (string)dr["up_filename"];
                    }
                    s.stf_id = (int)dr["stf_id"];
                    s.stf_u_id = (int)dr["stf_u_id"];
                    s.sc_name = (string)dr["sc_name"];
                    s.stf_fullname = (string)dr["stf_fullname"];
                    s.stf_email = (string)dr["stf_email"];
                    s.stf_contact = (string)dr["stf_contact"];
                    s.stf_address = (string)dr["stf_address"];
                    s.stf_shift = dr["stf_shift"].ToString();
                    s.stf_status = (short)dr["stf_status"];
                    stf.Add(s);
                }
            }
            _conn.Close();
            return stf;
        }

        public static UserStaff Read(string connStr, int stf_u_id)
        {
            UserStaff stf = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr =
                "SELECT * FROM db_perpus.staff "+
                "INNER JOIN db_perpus.users ON staff.stf_u_id = users.u_id "+
                "LEFT JOIN db_perpus.user_photo ON users.u_id = user_photo.up_u_id "+
                "WHERE (stf_u_id = '"+stf_u_id+"') AND (stf_rec_status = '1' AND u_rec_status = '1');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                stf = new UserStaff();
                stf.stf_id = _data.GetInt32("stf_id");
                stf.stf_u_id = _data.GetInt32("stf_u_id");
                stf.stf_sc_id = _data.GetInt32("stf_sc_id");
                stf.up_filename = _data.IsDBNull("up_filename") ? null : _data.GetString("up_filename");
                stf.up_photo = _data.IsDBNull("up_photo") ? null : (byte[])_data.GetValue("up_photo");
                stf.stf_fullname = _data.GetString("stf_fullname");
                stf.stf_email = _data.GetString("stf_email");
                stf.stf_contact = _data.GetString("stf_contact");
                stf.stf_address = _data.GetString("stf_address");
                stf.stf_shift = _data.GetValue("stf_shift").ToString();
                stf.stf_status = _data.GetInt16("stf_status");
                stf.u_id = _data.GetInt32("u_id");
                stf.u_uc_id = _data.GetInt32("u_uc_id");
                stf.u_username = _data.GetString("u_username").Substring(1);
                // stf.u_password = _data.GetString("u_password");
            }
            _conn.Close();
            return stf;
        }

        public static int Create(string connStr, UserStaff s)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "INSERT INTO `db_perpus`.`staff` "+
            "(`stf_id`,`stf_u_id`,`stf_sc_id`,`stf_fullname`,`stf_email`,`stf_contact`,`stf_address`,`stf_shift`,`stf_status`,`stf_rec_status`,`stf_rec_createdby`,`stf_rec_created`) "+
            "VALUES ('"+s.stf_id+"','"+s.stf_u_id+"','"+s.stf_sc_id+"','"+s.stf_fullname+"','"+s.stf_email+"','"+s.stf_contact+"','"+s.stf_address+"','"+s.stf_shift+"','"+s.stf_status+"','"+s.stf_rec_status+"','"+s.stf_rec_createdby+"','"+s.stf_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int CreateAlive(MySqlConnection _conn, UserStaff s)
        {
            int affectedRow = 0;

            string sqlStr = "INSERT INTO `db_perpus`.`staff` "+
            "(`stf_id`,`stf_u_id`,`stf_sc_id`,`stf_fullname`,`stf_email`,`stf_contact`,`stf_address`,`stf_shift`,`stf_status`,`stf_rec_status`,`stf_rec_createdby`,`stf_rec_created`) "+
            "VALUES ('"+s.stf_id+"','"+s.stf_u_id+"','"+s.stf_sc_id+"','"+s.stf_fullname+"','"+s.stf_email+"','"+s.stf_contact+"','"+s.stf_address+"','"+s.stf_shift+"','"+s.stf_status+"','"+s.stf_rec_status+"','"+s.stf_rec_createdby+"','"+s.stf_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool CreateStaffAndUser(string connStr, UserStaff s, Users u, UserPhoto up = null)
        {
            bool result = false;
            MySqlConnection _conn = null;
            MySqlCommand _cmd = null;
            MySqlTransaction sqlTrans = null;
            Random rand = null;
            try
            {
                rand = new Random();
                _cmd = new MySqlCommand();
                _conn = new MySqlConnection(connStr);
                _conn.Open();
                sqlTrans = _conn.BeginTransaction();
                _cmd.Transaction = sqlTrans;

                int affectedRow = 0;
                u.u_id = rand.Next(int.MinValue, int.MaxValue);
                affectedRow += UserCRUD.CreateAlive(_conn, u);
                if(up != null)
                {
                    up.up_id = rand.Next(int.MinValue, int.MaxValue);
                    up.up_u_id = u.u_id;
                    affectedRow += UserCRUD.CreatePhotoAlive(_conn, up);
                }
                s.stf_id = rand.Next(int.MinValue, int.MaxValue);
                s.stf_u_id = u.u_id;
                affectedRow += CreateAlive(_conn, s);

                if(affectedRow != 3-(up == null ? 1 : 0)) throw new Exception();

                sqlTrans.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                throw ex;
            }
            finally
            {
                if(sqlTrans != null)
                {
                    sqlTrans.Dispose();
                    sqlTrans = null;
                }

                if(_conn != null)
                {
                    _conn.Close();
                    _conn = null;
                    _cmd = null;
                }
            }
            return result;
        }

        public static int Update(string connStr, int stf_u_id, UserStaff s)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "UPDATE `db_perpus`.`staff` SET `stf_sc_id` = '"+s.stf_sc_id+"', `stf_fullname` = '"+s.stf_fullname+"', `stf_email` = '"+s.stf_email+"', `stf_contact` = '"+s.stf_contact+"', `stf_address` = '"+s.stf_address+"', `stf_shift` = '"+s.stf_shift+"', `stf_status` = '"+s.stf_status+"', `stf_rec_updatedby` = '"+s.stf_rec_updatedby+"', `stf_rec_updated` = '"+s.stf_rec_updated+"' WHERE (`stf_u_id` = '"+stf_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int UpdateAlive(MySqlConnection _conn, int stf_u_id, UserStaff s)
        {
            int affectedRow = 0;

            string sqlStr = "UPDATE `db_perpus`.`staff` SET `stf_sc_id` = '"+s.stf_sc_id+"', `stf_fullname` = '"+s.stf_fullname+"', `stf_email` = '"+s.stf_email+"', `stf_contact` = '"+s.stf_contact+"', `stf_address` = '"+s.stf_address+"', `stf_shift` = '"+s.stf_shift+"', `stf_status` = '"+s.stf_status+"', `stf_rec_updatedby` = '"+s.stf_rec_updatedby+"', `stf_rec_updated` = '"+s.stf_rec_updated+"' WHERE (`stf_u_id` = '"+stf_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool UpdateStaffAndUser(string connStr, UserStaff s, Users u, UserPhoto up = null)
        {
            bool result = false;
            MySqlConnection _conn = null;
            MySqlCommand _cmd = null;
            MySqlTransaction sqlTrans = null;
            try
            {
                _cmd = new MySqlCommand();
                _conn = new MySqlConnection(connStr);
                _conn.Open();
                sqlTrans = _conn.BeginTransaction();
                _cmd.Transaction = sqlTrans;

                int affectedRow = 0;
                if(up != null) affectedRow += UserCRUD.UpdatePhotoAlive(_conn, (int)u.u_id, up);
                affectedRow += UserCRUD.UpdateAlive(_conn, (int)u.u_id, u);
                affectedRow += UpdateAlive(_conn, (int)s.stf_u_id, s);

                if(affectedRow != 3-(up == null ? 1 : 0)) throw new Exception();

                sqlTrans.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                throw ex;
            }
            finally
            {
                if(sqlTrans != null)
                {
                    sqlTrans.Dispose();
                    sqlTrans = null;
                }

                if(_conn != null)
                {
                    _conn.Close();
                    _conn = null;
                    _cmd = null;
                }
            }
            return result;
        }

        public static int DeletePermanent(string connStr, int stf_u_id)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "DELETE FROM `db_perpus`.`staff` WHERE (`stf_u_id` = '"+stf_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int DeletePermanentAlive(MySqlConnection _conn, int stf_u_id)
        {
            int affectedRow = 0;

            string sqlStr = "DELETE FROM `db_perpus`.`staff` WHERE (`stf_u_id` = '"+stf_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool DeleteStaffAndUser(string connStr, int stf_u_id)
        {
            bool result = false;
            MySqlConnection _conn = null;
            MySqlCommand _cmd = null;
            MySqlTransaction sqlTrans = null;
            try
            {
                _cmd = new MySqlCommand();
                _conn = new MySqlConnection(connStr);
                _conn.Open();
                sqlTrans = _conn.BeginTransaction();
                _cmd.Transaction = sqlTrans;

                int affectedRow = 0;
                affectedRow += DeletePermanentAlive(_conn, stf_u_id);
                affectedRow += UserCRUD.DeleteAlive(_conn, stf_u_id);

                if(affectedRow != 2) throw new Exception();

                sqlTrans.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                throw ex;
            }
            finally
            {
                if(sqlTrans != null)
                {
                    sqlTrans.Dispose();
                    sqlTrans = null;
                }

                if(_conn != null)
                {
                    _conn.Close();
                    _conn = null;
                    _cmd = null;
                }
            }
            return result;
        }
    }
}