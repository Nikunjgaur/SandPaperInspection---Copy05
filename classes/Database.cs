﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace SandPaperInspection.classes
{
    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
    public class Database
    {
        // Process process = new Process();

        public void InsertRecord(DateTime date, DateTime time, string srNum, Point locationPoint,
                               string deftype, string imgPath, int productCode)
        {

            NpgsqlTypes.NpgsqlPoint location = new NpgsqlTypes.NpgsqlPoint(locationPoint.X, locationPoint.Y);

            using (NpgsqlConnection con = GetConnection())
            {
                
                string query = @"insert into public.logreport (_date, _time, serialNum, _location, deftype, imagepath, productcode)
                                values(@date, @time, @srNum, @location, @deftype, @imgPath, @productCode)";

                NpgsqlCommand cmd = new NpgsqlCommand(query, con);

                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@time", time);
                cmd.Parameters.AddWithValue("@srNum", srNum);
                cmd.Parameters.AddWithValue("@location", location);
                cmd.Parameters.AddWithValue("@deftype", deftype);
                //cmd.Parameters.AddWithValue("@defsize", defsize);
                cmd.Parameters.AddWithValue("@imgPath", imgPath);
                cmd.Parameters.AddWithValue("@productCode", productCode); 

                con.Open();
                int n = cmd.ExecuteNonQuery();
            }
        }

        public byte[] GetImageData()
        {
            byte[] imgData = null;
            using (NpgsqlConnection con = GetConnection())
            {

                string query = @"select * from public.gallery";

                NpgsqlCommand cmd = new NpgsqlCommand(query, con);

                con.Open();
                int n = cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    imgData = (byte[])reader[0];
                }
            }
            
            return imgData;
        }

        public void InsertImage(Bitmap inputImage)
        {

            using (NpgsqlConnection con = GetConnection())
            {

                string query = @"insert into public.gallery (imgdata) values(@bytes)";

                NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                cmd.Parameters.AddWithValue("@bytes", ImageExtensions.ToByteArray((Bitmap)inputImage.Clone(), ImageFormat.Bmp));

                con.Open();
                int n = cmd.ExecuteNonQuery();
            }


        }

        public void TestConnection()
        {
            using (NpgsqlConnection con = GetConnection())
            {
                con.Open();
                if (con.State != ConnectionState.Open)
                {
                    MessageBox.Show("Can not connect to Database");
                }
                else
                {
                    Console.WriteLine("Connected to database.");
                }
            }
        }

        public NpgsqlConnection GetConnection()
        {
            
            return new NpgsqlConnection(@"Server=localhost; Port = 5432; user Id = postgres; password = 1234; Database = sandpaper;");
        }
    }


}