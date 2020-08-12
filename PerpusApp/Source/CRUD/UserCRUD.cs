using System.Data;
using MySql.Data.MySqlClient;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.CRUD
{
    public sealed class UserCRUD
    {
        public static Users Read(string connStr, string u_username)
        {
            Users u = null;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "SELECT u_id, u_uc_id, u_username, u_password FROM db_perpus.users WHERE (`u_rec_status` = '1' AND `u_username` = '"+u_username+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            using MySqlDataReader _data = _cmd.ExecuteReader();
            if(_data.Read())
            {
                u = new Users();
                u.u_id = _data.GetInt32(0);
                u.u_uc_id = _data.GetInt32(1);
                u.u_username = _data.GetString(2);
                u.u_password = _data.GetString(3);
            }
            _conn.Close();
            return u;
        }

        public static int Create(string connStr, Users u)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlStr = "INSERT INTO `db_perpus`.`users`"+
            " (`u_id`, `u_uc_id`, `u_username`, `u_password`, `u_rec_status`, `u_rec_createdby`, `u_rec_created`)"+
            " VALUES ('"+u.u_id+"', '"+u.u_uc_id+"', '@"+u.u_username+"', '"+u.u_password+"', '"+u.u_rec_status+"', '"+u.u_rec_createdby+"', '"+u.u_rec_created+"');";
            
            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int CreateAlive(MySqlConnection _conn, Users u)
        {
            int affectedRow = 0;
            string sqlStr = "INSERT INTO `db_perpus`.`users`"+
            " (`u_id`, `u_uc_id`, `u_username`, `u_password`, `u_rec_status`, `u_rec_createdby`, `u_rec_created`)"+
            " VALUES ('"+u.u_id+"', '"+u.u_uc_id+"', '@"+u.u_username+"', '"+u.u_password+"', '"+u.u_rec_status+"', '"+u.u_rec_createdby+"', '"+u.u_rec_created+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        //CreatePhotoAlive?

        public static int Update(string connStr, int u_id, Users u)
        {
            int affectedRow = 0;
            using var _conn = new MySqlConnection(connStr);
            _conn.Open();

            string sqlPass = "";
            if(!u.u_password.Equals(null)) sqlPass = ", `u_password` = '"+u.u_password+"'";
            
            string sqlStr = "UPDATE `db_perpus`.`users` SET `u_username` = '"+u.u_username+"'"+sqlPass+", `u_rec_updatedby` = '"+u.u_rec_updatedby+"', `u_rec_updated` = '"+u.u_rec_updated+"' WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr, _conn);
            affectedRow = _cmd.ExecuteNonQuery();

            _conn.Close();
            return affectedRow;
        }

        public static int UpdateAlive(MySqlConnection _conn, int u_id, Users u)
        {
            int affectedRow = 0;

            string sqlPass = "";
            if(!u.u_password.Equals(null)) sqlPass = ", `u_password` = '"+u.u_password+"'";
            
            string sqlStr = "UPDATE `db_perpus`.`users` SET `u_username` = '"+u.u_username+"'"+sqlPass+", `u_rec_updatedby` = '"+u.u_rec_updatedby+"', `u_rec_updated` = '"+u.u_rec_updated+"' WHERE (`u_id` = '"+u_id+"');";

            using var _cmd = new MySqlCommand(sqlStr);
            _cmd.Connection = _conn;

            if(_conn.State == ConnectionState.Closed) _conn.Open();
            affectedRow = _cmd.ExecuteNonQuery();

            return affectedRow;
        }

        //UpdatePhotoAlive?

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

        //DeletePhotoAlive?
    }
}