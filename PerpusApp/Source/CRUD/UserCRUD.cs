using System;
using System.Data;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;
using PerpusApp.Source.General;

namespace PerpusApp.Source.CRUD
{
    public sealed class UserCRUD
    {
        public static Users Authenticate(string connStr, int u_id, string password)
        {
            Users u = null;

            u = Read(connStr, u_id);
            if (u == null)
                throw new Exception("Invalid Username!");
            else
                if (!Crypto.Verify(password, u.u_password)) return null;
            
            return u;
        }

        public static Users Authenticate(string connStr, string u_username, string password)
        {
            Users u = null;

            u = Read(connStr, u_username);
            if (u != null)
                if (!Crypto.Verify(password, u.u_password)) return null;
            
            return u;
        }

        public static Users Read(string connStr, int u_id)
        {
             Users u = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr =
                "SELECT * FROM db_perpus.users " +
                "WHERE (`u_rec_status` = '1' AND `u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                u = new Users();
                u.u_id = _data.GetInt32("u_id");
                u.u_uc_id = _data.GetInt32("u_uc_id");
                u.u_username = _data.GetString("u_username");
                u.u_password = _data.GetString("u_password");
            }
            _conn.Close();
            return u;
        }

        public static Users Read(string connStr, string u_username)
        {
            Users u = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr =
                "SELECT * FROM db_perpus.users " +
                "WHERE (`u_rec_status` = '1' AND `u_username` = '@"+u_username+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                u = new Users();
                u.u_id = _data.GetInt32("u_id");
                u.u_uc_id = _data.GetInt32("u_uc_id");
                u.u_username = _data.GetString("u_username");
                u.u_password = _data.GetString("u_password");
            }
            _conn.Close();
            return u;
        }

        public static int ReadRowData(string connStr, string u_username)
        {
            int rowReturned = 0;
            
            Users _data = Read(connStr, u_username);
            if (_data != null) rowReturned = 1; 

            return rowReturned;
        }

        public static int Create(string connStr, Users u)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr =
                "INSERT INTO `db_perpus`.`users`"+
                " (`u_id`, `u_uc_id`, `u_username`, `u_password`, "+
                "`u_rec_status`, `u_rec_createdby`, `u_rec_created`)"+
                " VALUES ('"+
                u.u_id+"', '"+u.u_uc_id+"', '@"+u.u_username+"', '"+u.u_password+"', '"+
                u.u_rec_status+"', '"+u.u_rec_createdby+"', '"+u.u_rec_created+"');";
            
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int CreateAlive(MySqlConnection _conn, Users u)
        {
            int affectedRow = 0;
            string sqlStr =
                "INSERT INTO `db_perpus`.`users`"+
                " (`u_id`, `u_uc_id`, `u_username`, `u_password`, `u_rec_status`, "+
                "`u_rec_createdby`, `u_rec_created`)"+
                " VALUES ('"+
                u.u_id+"', '"+u.u_uc_id+"', '@"+u.u_username+"', '"+u.u_password+"', '"+
                u.u_rec_status+"', '"+u.u_rec_createdby+"', '"+u.u_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static int ReadPhoto(string connStr, int up_u_id)
        {
            int rowReturned = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "SELECT * FROM db_perpus.user_photo WHERE up_u_id = '"+up_u_id+"'";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();

            if(_data.HasRows) rowReturned = 1;

            _conn.Close();
            return rowReturned;
        }

        public static int CreatePhotoAlive(MySqlConnection _conn, UserPhoto up)
        {
            int affectedRow = 0;
            string sqlStr =
                "INSERT INTO `db_perpus`.`user_photo` "+
                "(`up_id`,`up_u_id`,`up_photo`,`up_filename`,`up_rec_status`) "+
                "VALUES ('"+
                up.up_id+"','"+up.up_u_id+"','"+up.up_photo+"','"+up.up_filename+"','"+up.up_rec_status+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            if(affectedRow == 1) affectedRow = UpdatePhotoAlive(_conn, (int)up.up_u_id, up);
            return affectedRow;
        }

        public static int UpdatePhotoAlive(MySqlConnection _conn, int u_id, UserPhoto up)
        {
            int affectedRow = 0;
            string sqlStr =
                "UPDATE `db_perpus`.`user_photo` SET "+
                "`up_photo` = '"+up.up_photo+"', `up_filename` = '"+up.up_filename+"' "+
                "WHERE up_u_id = '"+u_id+"'";

            using var _cmd = new MySqlCommand();
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            _cmd.CommandText = sqlStr;
            affectedRow = _cmd.ExecuteNonQuery();

            if(affectedRow == 1)
            {
                sqlStr = "UPDATE `db_perpus`.`user_photo` SET `up_photo` = @photo"+
                            " WHERE up_u_id = "+ u_id.ToString();
                _cmd.Parameters.Add("@photo", MySqlDbType.Blob, up.up_photo.Length).Value = up.up_photo;
                _cmd.CommandText = sqlStr;
                affectedRow = _cmd.ExecuteNonQuery();
                _cmd.Parameters.Clear();
            }
            return affectedRow;
        }

        public static bool CreatePhoto(string connStr, UserPhoto up, int u_id)
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
                up.up_id = rand.Next(int.MinValue, int.MaxValue);
                up.up_u_id = u_id;
                affectedRow += CreatePhotoAlive(_conn, up);

                if(affectedRow != 1) throw new Exception();
                
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

        public static int Update(string connStr, int u_id, Users u)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlPass = (u.u_password == null) ? "" : "`u_password` = '"+u.u_password+"'";
            string sqlUsername = (u.u_username == null) ? "" : "`u_username` = '@"+u.u_username+"'";
            string sqlComma = (sqlUsername == "") ? "" : ", ";
            
            string sqlStr =
                "UPDATE `db_perpus`.`users` SET "+
                sqlUsername + sqlComma + sqlPass +
                ", `u_rec_updatedby` = '"+u.u_rec_updatedby+"', `u_rec_updated` = '"+u.u_rec_updated+"' "+
                "WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int UpdateAlive(MySqlConnection _conn, int u_id, Users u)
        {
            int affectedRow = 0;

            string sqlPass = "";
            if(u.u_password != null) sqlPass = ", `u_password` = '"+u.u_password+"'";
            
            string sqlStr =
                "UPDATE `db_perpus`.`users` SET "+
                "`u_username` = '@"+u.u_username+"'"+sqlPass+", "+
                "`u_rec_updatedby` = '"+u.u_rec_updatedby+"', `u_rec_updated` = '"+u.u_rec_updated+"' "+
                "WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        public static int Delete(string connStr, int u_id)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();
            string sqlStr = "DELETE FROM `db_perpus`.`users` WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int DeleteAlive(MySqlConnection _conn, int u_id)
        {
            int affectedRow = 0;

            string sqlStr = "DELETE FROM `db_perpus`.`users` WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }
    }
}