using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Configuration;

namespace StarReportingCorrection
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["SourceConnection"].ToString();
                string connectionStringProd = ConfigurationManager.ConnectionStrings["SourceConnection1"].ToString();
                string SerialNumberSQLCommsand = ConfigurationManager.AppSettings.Get("SerialNumberCorrectionQuery").ToString();
                string REOpenOrderSQLCommsand = ConfigurationManager.AppSettings.Get("REOpenOrderCorrectionQuery").ToString();
                string POSQLCommsand = ConfigurationManager.AppSettings.Get("POCorrectionQuery").ToString();
                string POLineSQLCommsand = ConfigurationManager.AppSettings.Get("POLineCorrectionQuery").ToString();
                string POLineUpdateSQLCommsand = ConfigurationManager.AppSettings.Get("POLineCorrectionUpdateQuery").ToString();
                string PartOnlySQLCommsand = ConfigurationManager.AppSettings.Get("PartOnlyQuery").ToString();
                string MailBodyString = null;
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand comm = new SqlCommand(SerialNumberSQLCommsand, conn);
                comm.CommandTimeout = 300000;
                conn.Open();
                MailBodyString = "Hi Team, <br/><br/> Below are the stats that are corrected as part of reporting correction. <br/><br/>";
                MailBodyString += "<li> Serial number correction: " + comm.ExecuteNonQuery() + "</li>";

                comm = new SqlCommand(REOpenOrderSQLCommsand, conn);
                comm.CommandTimeout = 300000;
                MailBodyString += "<li> RE Open Order report correction: " + comm.ExecuteNonQuery() + "</li>";

                comm = new SqlCommand(POSQLCommsand, conn);
                comm.CommandTimeout = 300000;
                MailBodyString += "<li> Purchase Order Report correction: " + comm.ExecuteNonQuery() + "</li>";

                comm = new SqlCommand(POLineSQLCommsand, conn);
                int Value = comm.ExecuteNonQuery();
                //MailBodyString += "<li> Purchase Order Line correction: " + comm.ExecuteNonQuery() + "</li>";

                SqlDataAdapter POLineCheckAdapter = new SqlDataAdapter(comm);
                comm.CommandTimeout = 300000;
                DataSet POLineCheckSet = new DataSet();
                POLineCheckAdapter.Fill(POLineCheckSet);
                if (POLineCheckSet.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < POLineCheckSet.Tables[0].Rows.Count; i++)
                    {
                        POLineUpdateSQLCommsand += POLineCheckSet.Tables[0].Rows[i][0].ToString();
                        POLineUpdateSQLCommsand += "','";
                    }
                }
                POLineUpdateSQLCommsand = POLineUpdateSQLCommsand.Substring(0, POLineUpdateSQLCommsand.Length - 2);
                if (POLineCheckSet.Tables[0].Rows.Count > 0)
                {
                    comm = new SqlCommand(POLineUpdateSQLCommsand + ")", conn);
                    comm.CommandTimeout = 300000;

                    MailBodyString += "<li> Purchase Order Line corrected: " + comm.ExecuteNonQuery() + "</li>";
                }
                else {
                    MailBodyString += "<li> Purchase Order Line corrected: 0 </li>";
                }
                    conn = new SqlConnection(connectionStringProd);
                    comm = new SqlCommand(PartOnlySQLCommsand, conn);
                    POLineCheckAdapter = new SqlDataAdapter(comm);
                    comm.CommandTimeout = 300000;
                    POLineCheckSet = new DataSet();
                    POLineCheckAdapter.Fill(POLineCheckSet);
                
                    MailBodyString += "<BR/><BR/><li> Part Only orders in Open State: " + POLineCheckSet.Tables[0].Rows[0][0].ToString() + "</li>";

                    MailBodyString += "<br/><br/>Thanks and regards,<br/>STAR ASM";
                    SendEmailHelper SendMail = new SendEmailHelper();
                    SendMail.SendEmail(MailBodyString);
                
                
            }
            catch (Exception ex)
            {

                string strToAddress = ConfigurationManager.AppSettings.Get("ToAddress").ToString();
                string strFromAddress = ConfigurationManager.AppSettings.Get("FromAddress").ToString();
                MailMessage message = new MailMessage(strToAddress, strFromAddress, "Exception Occurred while STAR Report correction", ex.StackTrace);
                message.Priority = MailPriority.High;
                message.IsBodyHtml = true;
                message.Headers.Add("Reply-To", "mukeshg.nadar@bestbuy.com");
                SmtpClient emailClient = new SmtpClient(ConfigurationManager.AppSettings.Get("SMTPServer").ToString());
                emailClient.Send(message);
            }
        }
    }
}
