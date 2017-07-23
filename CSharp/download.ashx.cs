using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DRAP.Wcf
{
    /// <summary>
    /// Summary description for Download
    /// </summary>
    public class Download : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;


            var fileName = request["File"];

            if (string.IsNullOrEmpty(fileName))
                throw new Exception("File name is Empty");


            DownloadBlobUtil downloadBlobUtil = new DownloadBlobUtil();
            byte[] buffer = downloadBlobUtil.GetBytesfromBlobUrl(fileName);


            context.Response.Clear();
            context.Response.ContentType = "application/pdf";

            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName + ".pdf;");
            //context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.BinaryWrite(buffer);
            context.Response.Flush();
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}