using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.CRUD
{
    public sealed class SiteCRUD
    {
        public static List<UserSite> ReadAll(string connStr)
        {
            List<UserSite> siteList = new List<UserSite>();
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr =
                "SELECT * FROM `db_perpus`.`site` "+
                "INNER JOIN `db_perpus`.`users` ON users.u_id = site.s_u_id "+
                "LEFT JOIN `db_perpus`.`user_photo` ON user_photo.up_u_id = users.u_id "+
                "WHERE (`s_rec_status` = '1');";
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            DataTable dt = new DataTable();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_cmd);
            dataAdapter.Fill(dt);
            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    UserSite s = new UserSite();
                    if(dr["up_id"] != DBNull.Value)
                    {
                        s.up_photo = (byte[])dr["up_photo"];
                        s.up_filename = (string)dr["up_filename"];
                    }
                    s.s_id = (int)dr["s_id"];
                    s.s_u_id = (int)dr["s_u_id"];
                    s.s_fullname = (string)dr["s_fullname"];
                    s.s_email = (string)dr["s_email"];
                    s.s_contact = (string)dr["s_contact"];
                    s.s_address = (string)dr["s_address"];
                    s.s_status = (short)dr["s_status"];
                    siteList.Add(s);
                }
            }
            _conn.Close();
            return siteList;
        }

        public static UserSite Read(string connStr, int s_u_id)
        {
            UserSite ust = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "SELECT * FROM db_perpus.site "+
                            "INNER JOIN db_perpus.users ON users.u_id = site.s_u_id "+
                            "LEFT JOIN db_perpus.user_photo ON user_photo.up_u_id = users.u_id "+
                            "WHERE (s_u_id = '"+s_u_id+"') AND (s_rec_status = '1' AND u_rec_status = '1');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                ust = new UserSite();
                ust.s_id = _data.GetInt32("s_id");
                ust.s_u_id = _data.GetInt32("s_u_id");
                ust.up_filename = _data.IsDBNull("up_filename") ? null : _data.GetString("up_filename");
                ust.up_photo = _data.IsDBNull("up_photo") ? null : (byte[])_data.GetValue("up_photo");
                ust.s_fullname = _data.GetString("s_fullname");
                ust.s_email = _data.GetString("s_email");
                ust.s_contact = _data.GetString("s_contact");
                ust.s_address = _data.GetString("s_address");
                ust.s_status = _data.GetInt16("s_status");
                ust.u_id = _data.GetInt32("u_id");
                ust.u_uc_id = _data.GetInt32("u_uc_id");
                ust.u_username = _data.GetString("u_username").Substring(1);
                // ust.u_password = _data.GetString("u_password");
            }
            _conn.Close();
            return ust;
        }

        public static int Create(string connStr, UserSite s)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "INSERT INTO `db_perpus`.`site` "+
            "(`s_id`,`s_u_id`,`s_fullname`,`s_email`,`s_contact`,`s_address`,`s_status`,`s_rec_status`,`s_rec_createdby`,`s_rec_created`) "+
            "VALUES ('"+s.s_id+"','"+s.s_u_id+"','"+s.s_fullname+"','"+s.s_email+"','"+s.s_contact+"','"+s.s_address+"','"+s.s_status+"','"+s.s_rec_status+"','"+s.s_rec_createdby+"','"+s.s_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int CreateAlive(MySqlConnection _conn, UserSite s)
        {
            int affectedRow = 0;

            string sqlStr = "INSERT INTO `db_perpus`.`site` "+
            "(`s_id`,`s_u_id`,`s_fullname`,`s_email`,`s_contact`,`s_address`,`s_status`,`s_rec_status`,`s_rec_createdby`,`s_rec_created`) "+
            "VALUES ('"+s.s_id+"','"+s.s_u_id+"','"+s.s_fullname+"','"+s.s_email+"','"+s.s_contact+"','"+s.s_address+"','"+s.s_status+"','"+s.s_rec_status+"','"+s.s_rec_createdby+"','"+s.s_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool CreateSiteAndUser(string connStr, UserSite s, Users u, UserPhoto up = null)
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
                s.s_id = rand.Next(int.MinValue, int.MaxValue);
                s.s_u_id = u.u_id;
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

        public static int Update(string connStr, int s_u_id, UserSite s)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr = "UPDATE `db_perpus`.`site` SET `s_fullname` = '"+s.s_fullname+"', `s_email` = '"+s.s_email+"', `s_contact` = '"+s.s_contact+"', `s_address` = '"+s.s_address+"', `s_status` = '"+s.s_status+"', `s_rec_updatedby` = '"+s.s_rec_updatedby+"', `s_rec_updated` = '"+s.s_rec_updated+"' WHERE (`s_u_id` = '"+s_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int UpdateAlive(MySqlConnection _conn, int s_u_id, UserSite s)
        {
            int affectedRow = 0;
            string sqlStr = "UPDATE `db_perpus`.`site` SET `s_fullname` = '"+s.s_fullname+"', `s_email` = '"+s.s_email+"', `s_contact` = '"+s.s_contact+"', `s_address` = '"+s.s_address+"', `s_status` = '"+s.s_status+"', `s_rec_updatedby` = '"+s.s_rec_updatedby+"', `s_rec_updated` = '"+s.s_rec_updated+"' WHERE (`s_u_id` = '"+s_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool UpdateSiteAndUser(string connStr, UserSite s, Users u, UserPhoto up = null)
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
                affectedRow += UpdateAlive(_conn, (int)s.s_u_id, s);

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

        public static int DeleteTemporary(string connStr, int s_id, UserSite s)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "UPDATE `db_perpus`.`site` SET "+
                            "`s_rec_status` = '"+s.s_rec_status+"',"+
                            "`s_rec_deletedby` = '"+s.s_rec_deletedby+"',"+
                            "`s_rec_deleted` = '"+s.s_rec_deleted+"';";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int DeleteTemporaryAlive(MySqlConnection _conn, int s_id, UserSite s)
        {
            int affectedRow = 0;

            string sqlStr = "UPDATE `db_perpus`.`site` SET "+
                            "`s_rec_status` = '"+s.s_rec_status+"',"+
                            "`s_rec_deletedby` = '"+s.s_rec_deletedby+"',"+
                            "`s_rec_deleted` = '"+s.s_rec_deleted+"';";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static int DeletePermanent(string connStr, int s_u_id)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "DELETE FROM `db_perpus`.`site` WHERE (`s_u_id` = '"+s_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int DeletePermanentAlive(MySqlConnection _conn, int s_u_id)
        {
            int affectedRow = 0;

            string sqlStr = "DELETE FROM `db_perpus`.`site` WHERE (`s_u_id` = '"+s_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool DeleteSiteAndUser(string connStr, int s_u_id)
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
                affectedRow += DeletePermanentAlive(_conn, s_u_id);
                affectedRow += UserCRUD.DeleteAlive(_conn, s_u_id);

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