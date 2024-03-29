﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using RestSharp;
using RestSharp.Serialization;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace KoperasiDataSyncronization
{
    
    public class VMSAPIModel
    {
        public string status;
        public string message;
        public string[] data;
    }

    class VMSAPIService
    {
        DataEncryption dataEncryption;
        string conString;
        public string terminanteUserResult;
        public string updateUserResult;

        public VMSAPIService()
        {
            Console.WriteLine("VMS API Data Syncronizer Services - Started -");
            dataEncryption = new DataEncryption(ConfigurationManager.AppSettings["VMSJOB_Key"], ConfigurationManager.AppSettings["VMSJOB_IV"]);
            conString = ConfigurationManager.AppSettings["ConfigDBServer"];

            VMSAPITerminateUser();
            VMSAPIUpdateDataUser();
        }

        public List<VMSTerminateModel> GetEmployeeTerminate()
        {
            List<VMSTerminateModel> TerminateModel = new List<VMSTerminateModel>();
            SqlConnection con = new SqlConnection(conString);
            try
            {
                SqlCommand com = new SqlCommand("SELECT * FROM VIEW_vms_emp_terminate", con);
                com.CommandTimeout = 0;
                com.CommandType = CommandType.Text;
                

                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    VMSTerminateModel model = new VMSTerminateModel();
                    model.PersonID = reader["person_id"].ToString();
                    model.NIK = reader["NIK"].ToString();
                    model.Name = reader["Name"].ToString();
                    model.CompanyName = reader["UnitBusiness"].ToString();
                    model.ResignDate = reader["resign_date"].ToString();
                    model.reason = reader["resign_reason"].ToString();

                    TerminateModel.Add(model);
                }
            }
            catch (Exception err)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }

            return TerminateModel;
        }

        public bool UpdateFlagEmpTerminate(VMSTerminateModel model)
        {
            bool result = false;
            SqlConnection con = new SqlConnection(conString);
            try
            {
                SqlCommand com = new SqlCommand("UPDATE [dbo].[emp_terminate] SET [flag_vms_sync] = '" + model.flag_vms_sync + "'  WHERE [person_id] = '" + model.PersonID + "' AND [resign_date] = '" + model.ResignDate + "'", con);
                com.CommandTimeout = 0;
                com.CommandType = CommandType.Text;
                
                con.Open();
                if (com.ExecuteNonQuery() > 0)
                {
                    result = true;
                }                
            }
            catch (Exception err)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }

            return result;
        }

        public Boolean VMSAPITerminateUser()
        {
            bool result = false;
            List<string> message = new List<string>();
            foreach(VMSTerminateModel model in GetEmployeeTerminate())
            {
                var client = new RestClient("http://117.102.82.138");
                var request = new RestRequest("erp/api_vms_request_noactive.php", Method.GET);

                request.AddParameter("hash", dataEncryption.Encrypt(DateTime.Now.ToString("yyyy-MM-dd"))); // adds to POST or URL querystring based on Method
                request.AddParameter("user", dataEncryption.Encrypt("api.koperasi.local"));
                request.AddParameter("NIK", model.NIK);
                request.AddParameter("name", model.Name);
                request.AddParameter("company", model.CompanyName);
                request.AddParameter("msg", model.reason);

                var response = client.Execute(request);
                terminanteUserResult = response.Content;
                JObject data = (JObject)JsonConvert.DeserializeObject(response.Content);
                message.Add(data["message"].ToString());
                if(data["status"].ToString() == "1")
                {
                    model.flag_vms_sync = "1";
                    
                    result = UpdateFlagEmpTerminate(model);
                    if (!result)
                    {
                        return result;
                    }
                }
                
            }
            

            return result;
        }

        public Boolean VMSAPIUpdateDataUser()
        {
            bool result = false;
            var client = new RestClient("http://117.102.82.138");
            var request = new RestRequest("erp/api_vms_update_data.php", Method.GET);

            request.AddParameter("hash", dataEncryption.Encrypt(DateTime.Now.ToString("yyyy-MM-dd"))); // adds to POST or URL querystring based on Method
            request.AddParameter("user", dataEncryption.Encrypt("api.koperasi.local"));
            request.AddParameter("date", DateTime.Now.ToString("yyyy-MM-dd"));

            var response = client.Execute(request);
            updateUserResult = response.Content;
            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            VMSDataModel model = new VMSDataModel();
            if (data["data"] != null)
            {
                foreach (var item in data["data"])
                {
                    model.NIK = item[3].ToString();
                    model.Name = item[1].ToString();
                    model.UnitBusiness = item[2].ToString();
                    model.CardNo = item[0].ToString();
                    model.Photo = item[4].ToString();

                    result = UpdateDataVMS(model);

                    if (!result)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        public Boolean UpdateDataVMS(VMSDataModel model)
        {

            SqlConnection con = new SqlConnection(conString);
            try
            {
                SqlCommand com = new SqlCommand("SP_UpdateVMSData", con);
                com.CommandTimeout = 0;
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.Add(new SqlParameter("@NIK", model.NIK));
                com.Parameters.Add(new SqlParameter("@Photo", model.Photo));
                com.Parameters.Add(new SqlParameter("@CardNo", model.CardNo));
                com.Parameters.Add(new SqlParameter("@Name", model.Name));
                com.Parameters.Add(new SqlParameter("@UnitBusiness", model.UnitBusiness));

                con.Open();
                com.ExecuteNonQuery();

            }
            catch (Exception err)
            {
                con.Close();
                return false;
            }
            finally
            {
                con.Close();
            }

            return true;
        }


    }
}
