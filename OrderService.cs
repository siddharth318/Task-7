using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Task_7
{
    public class OrderService
    {
        [FunctionName("OrderService")]
        public void Run([ServiceBusTrigger("paymnt-queue", Connection = "conString")]string myQueueItem, ILogger log)
        {
            log.LogInformation("Order MS Ran");

            dynamic data = JsonConvert.DeserializeObject(myQueueItem);

            string prodID = data.ProdID;
            string prodCost = data.ProdCost;           
            string custEmail = "siddharthd@cloudthat.com";
            int result = 0;

            //SQL connection
            SqlConnection conObj = new SqlConnection("Data Source=quickcart-server.database.windows.net;Initial Catalog=QuickCart-DB;user id=demouser; password=Siddharth@1234");

            //command
            SqlCommand cmdObj = new SqlCommand("usp_AddOrder", conObj);
            cmdObj.CommandType = CommandType.StoredProcedure;

            try
            {
                SqlParameter emailPrm = new SqlParameter();
                emailPrm.ParameterName = "@custEmail";
                emailPrm.Value = custEmail;
                emailPrm.Direction = ParameterDirection.Input;
                emailPrm.SqlDbType = SqlDbType.VarChar;

                SqlParameter idPrm = new SqlParameter();
                idPrm.ParameterName = "@prodId";
                idPrm.Value = prodID;
                idPrm.Direction = ParameterDirection.Input;
                idPrm.SqlDbType = SqlDbType.Int;

                SqlParameter costPrm= new SqlParameter();
                costPrm.ParameterName = "@prodCost";
                costPrm.Value = prodCost;
                costPrm.Direction = ParameterDirection.Input;
                costPrm.SqlDbType = SqlDbType.Int;

                SqlParameter orderDatePrm = new SqlParameter();
                orderDatePrm.ParameterName = "@orderdate";
                orderDatePrm.Value = DateTime.Now;
                orderDatePrm.Direction = ParameterDirection.Input;
                orderDatePrm.SqlDbType = SqlDbType.DateTime;


                cmdObj.Parameters.Add(emailPrm);
                cmdObj.Parameters.Add(idPrm);
                cmdObj.Parameters.Add(costPrm);
                cmdObj.Parameters.Add(orderDatePrm);


                SqlParameter prmReturnValue = new SqlParameter();
                prmReturnValue.Direction = ParameterDirection.ReturnValue;
                cmdObj.Parameters.Add(prmReturnValue);
                conObj.Open();
                cmdObj.ExecuteNonQuery();
                int res = Convert.ToInt32(prmReturnValue.Value);
                if (res == 1)
                    result = 1;//it means added
                else
                    result = 0;//error
            }
            catch (Exception e)
            {
                result = -1;

            }
            finally
            {
                conObj.Close();
            }

            log.LogInformation("" + result);
        }
    }
}
