using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Intelligent.OTC.Common.Utils;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.Dtos;


namespace Intelligent.OTC.Domain.DataModel
{
    public class ReconServiceProxy
    {
        public ReconServiceProxy(string reconServiceEndPoint)
        {
            this.reconServiceEndPoint = reconServiceEndPoint;
            client = new HttpClient() { Timeout = new TimeSpan(0, 20, 0) };
        }

        HttpClient client = null;
        string reconServiceEndPoint { get; set; }

        public string cleanData(CaTaskMsg msg)
        {
            try
            {
                Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                task.Wait();

                var res = task.Result.Content.ReadAsAsync<CaTaskResult>();
                res.Wait();

                return res.Result.result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Clean data failed", ex);
                throw;
            }
        }

        public string identifyCustomer(CaTaskMsg msg)
        {
            try
            {
                client.PostAsJsonAsync(reconServiceEndPoint, msg);
                return "success";
                //Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                //task.Wait();

                //var res = task.Result.Content.ReadAsAsync<CaTaskResult>();
                //res.Wait();

                //return res.Result.result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("IdentifyCustomer method run failed", ex);
                throw;
            }
        }

        public string recon(CaTaskMsg msg)
        {
            try
            {
                client.PostAsJsonAsync(reconServiceEndPoint, msg);
                return "";

                //Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                //task.Wait();

                //var res = task.Result.Content.ReadAsAsync<CaTaskResult>();
                //res.Wait();

                //return res.Result.result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Recon method run failed", ex);
                throw;
            }
        }

        public string paymentDetailRecon(CaTaskMsg msg)
        {
            try
            {
                client.PostAsJsonAsync(reconServiceEndPoint, msg);

                Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                task.Wait();

                var res = task.Result.Content.ReadAsAsync<CaTaskResult>();
                res.Wait();

                return res.Result.result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Recon method run failed", ex);
                throw;
            }
        }

        public string unknownCashAdvisor(CaTaskMsg msg)
        {
            try
            {
                client.PostAsJsonAsync(reconServiceEndPoint, msg);
                return "success";

                //Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                //task.Wait();

                //var res = task.Result.Content.ReadAsAsync<CaTaskResult>();
                //res.Wait();

                //return res.Result.result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Unknown Cash Advisor method run failed", ex);
                throw;
            }
        }

        public CaReconMsgResultDto splitRecon(CaReconMsgDto msg)
        {
            try
            {
                Task<HttpResponseMessage> task = client.PostAsJsonAsync(reconServiceEndPoint, msg);

                task.Wait();

                var res = task.Result.Content.ReadAsAsync<CaReconMsgResultDto>();
                res.Wait();

                return res.Result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Clean data failed", ex);
                throw;
            }
        }

    }
}
