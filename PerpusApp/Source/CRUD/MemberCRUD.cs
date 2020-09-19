using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.CRUD
{
    public sealed class MemberCRUD
    {
        public static List<UserMember> ReadAll(string connStr)
        {
            List<UserMember> members = new List<UserMember>();
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = 
                "SELECT * FROM `db_perpus`.`member` "+
                "INNER JOIN db_perpus.users ON `member`.m_u_id = users.u_id "+
                "LEFT JOIN db_perpus.user_photo ON users.u_id = user_photo.up_u_id "+
                "WHERE (m_rec_status = '1');";
            
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            DataTable dt = new DataTable();
            
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_cmd);
            dataAdapter.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    UserMember m = new UserMember();
                    if(dr["up_id"] !=  DBNull.Value)
                    {
                        m.up_photo = (byte[])dr["up_photo"];
                        m.up_filename = (string)dr["up_filename"];
                    }
                    m.m_id = (int)dr["m_id"];
                    m.m_u_id = (int)dr["m_u_id"];
                    m.m_class = (string)dr["m_class"];
                    m.m_fullname = (string)dr["m_fullname"];
                    m.m_email = (string)dr["m_email"];
                    m.m_contact = (string)dr["m_contact"];
                    m.m_address = (string)dr["m_address"];
                    m.m_status = (short)dr["m_status"];
                    members.Add(m);
                }
            }
            _conn.Close();
            return members;
        }

        public static UserMember Read(string connStr, int m_u_id)
        {
            UserMember m = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = 
                "SELECT * FROM `db_perpus`.`member` "+
                "INNER JOIN db_perpus.users ON `member`.m_u_id = users.u_id "+
                "LEFT JOIN db_perpus.user_photo ON users.u_id = user_photo.up_u_id "+
                "WHERE (m_u_id = '"+m_u_id+"') AND (m_rec_status = '1' AND u_rec_status = '1');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if (_data.Read())
            {
                m = new UserMember();
                m.m_id = _data.GetInt32("m_id");
                m.m_u_id = _data.GetInt32("m_u_id");
                m.up_filename = _data.IsDBNull("up_filename") ? null : _data.GetString("up_filename");
                m.up_photo = _data.IsDBNull("up_photo") ? null : (byte[])_data.GetValue("up_photo");
                m.m_class = _data.GetString("m_class");
                m.m_fullname = _data.GetString("m_fullname");
                m.m_email = _data.GetString("m_email");
                m.m_contact = _data.GetString("m_contact");
                m.m_address = _data.GetString("m_address");
                m.m_status = _data.GetInt16("m_status");
                m.u_id = _data.GetInt32("u_id");
                m.u_uc_id = _data.GetInt32("u_uc_id");
                m.u_username = _data.GetString("u_username").Substring(1);
            }
            _conn.Close();
            return m;
        }

        public static int Create(string connStr, UserMember m)
        {
            int affectedRow = 0;using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr =
                "INSERT INTO `db_perpus`.`member` "+
                "(`m_id`,`m_u_id`,"+
                "`m_class`,`m_fullname`,`m_email`,`m_contact`,`m_address`,`m_status`,"+
                "`m_rec_status`,`m_rec_createdby`,`m_rec_created`) "+
                "VALUES ('"+m.m_id+"','"+m.m_u_id+"','"+
                m.m_class+m.m_fullname+"','"+m.m_email+"','"+m.m_contact+"','"+m.m_address+"','"+
                m.m_status+"','"+m.m_rec_status+"','"+m.m_rec_createdby+"','"+m.m_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int CreateAlive(MySqlConnection _conn, UserMember m)
        {
            int affectedRow = 0;

            string sqlStr =
                "INSERT INTO `db_perpus`.`member` "+
                "(`m_id`,`m_u_id`,"+
                "`m_class`,`m_fullname`,`m_email`,`m_contact`,`m_address`,`m_status`,"+
                "`m_rec_status`,`m_rec_createdby`,`m_rec_created`) "+
                "VALUES ('"+m.m_id+"','"+m.m_u_id+"','"+
                m.m_class+m.m_fullname+"','"+m.m_email+"','"+m.m_contact+"','"+m.m_address+"','"+
                m.m_status+"','"+m.m_rec_status+"','"+m.m_rec_createdby+"','"+m.m_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool CreateMemberAndUser(string connStr, UserMember m, Users u, UserPhoto up = null)
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
                m.m_id = rand.Next(int.MinValue, int.MaxValue);
                m.m_u_id = u.u_id;
                affectedRow += CreateAlive(_conn, m);

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

        public static int Update(string connStr, int m_u_id, UserMember m)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = 
                "UPDATE `db_perpus`.`member` SET "+
                "`m_class` = '"+m.m_class+
                "', `m_fullname` = '"+m.m_fullname+
                "', `m_email` = '"+m.m_email+
                "', `m_contact` = '"+m.m_contact+
                "', `m_address` = '"+m.m_address+
                "', `m_status` = '"+m.m_status+
                "', `m_rec_updatedby` = '"+m.m_rec_updatedby+
                "', `m_rec_updated` = '"+m.m_rec_updated+
                "' WHERE (`m_u_id` = '"+m_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int UpdateAlive(MySqlConnection _conn, int m_u_id, UserMember m)
        {
            int affectedRow = 0;

            string sqlStr = 
                "UPDATE `db_perpus`.`member` SET "+
                "`m_class` = '"+m.m_class+
                "', `m_fullname` = '"+m.m_fullname+
                "', `m_email` = '"+m.m_email+
                "', `m_contact` = '"+m.m_contact+
                "', `m_address` = '"+m.m_address+
                "', `m_status` = '"+m.m_status+
                "', `m_rec_updatedby` = '"+m.m_rec_updatedby+
                "', `m_rec_updated` = '"+m.m_rec_updated+
                "' WHERE (`m_u_id` = '"+m_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool UpdateMemberAndUser(string connStr, UserMember m, Users u, UserPhoto up = null)
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
                affectedRow += UpdateAlive(_conn, (int)m.m_u_id, m);

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

        public static int Delete(string connStr, int m_u_id)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "DELETE FROM `db_perpus`.`member` WHERE (`m_u_id` = '"+m_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int DeleteAlive(MySqlConnection _conn, int m_u_id)
        {
            int affectedRow = 0;

            string sqlStr = "DELETE FROM `db_perpus`.`member` WHERE (`m_u_id` = '"+m_u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static bool DeleteMemberAndUser(string connStr, int m_u_id)
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
                affectedRow += DeleteAlive(_conn, m_u_id);
                affectedRow += UserCRUD.DeleteAlive(_conn, m_u_id);

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