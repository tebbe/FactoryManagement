using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
//using WebMatrix.WebData;
using FactoryManagement.Models;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace FactoryManagement.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EncryptedActionParameterAttribute : ActionFilterAttribute
    {
        public static byte[] _keyBytes = { 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18 };
        public static string _keyString = "SB@45604560";
        public static string _checksumKey = "__$$";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();
            Dictionary<string, object> parametersType = new Dictionary<string, object>();
            if (HttpContext.Current.Request.QueryString.Get("q") != null)
            {
                ReflectedActionDescriptor ActionInfo = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
                List<ParameterDescriptor> ExpectedParameters = ActionInfo.GetParameters().ToList();
                foreach (var p in ExpectedParameters)
                {
                    if (p.ParameterType.Name == "Nullable`1")
                    {
                        parametersType[p.ParameterName] = p.ParameterType.FullName.Split(',')[0].Split('[')[2];
                    }
                    else
                    {
                        parametersType[p.ParameterName] = p.ParameterType.FullName;
                    }
                }

                string encryptedQueryString = HttpContext.Current.Request.QueryString.Get("q");
                string decrptedString = Decrypt(encryptedQueryString.ToString());
                string[] paramsArrs = decrptedString.Split('?');

                for (int i = 0; i < paramsArrs.Length; i++)
                {
                    string[] paramArr = paramsArrs[i].Split('=');
                    if (parametersType.ContainsKey(paramArr[0]))
                    {
                        var type = parametersType[paramArr[0]].ToString();
                        type = type.Split('.')[1];
                        if (type == "Int32")
                        {
                            decryptedParameters.Add(paramArr[0], Convert.ToInt32(paramArr[1]));
                        }
                        else if (type == "Int64")
                        {
                            decryptedParameters.Add(paramArr[0], Convert.ToInt64(paramArr[1]));
                        }
                        else if (type == "Boolean")
                        {
                            decryptedParameters.Add(paramArr[0], Convert.ToBoolean(paramArr[1]));
                        }
                        else if (type == "String")
                        {
                            decryptedParameters.Add(paramArr[0], (paramArr[1]).ToString());
                        }

                    }
                }
            }
            for (int i = 0; i < decryptedParameters.Count; i++)
            {
                filterContext.ActionParameters[decryptedParameters.Keys.ElementAt(i)] = decryptedParameters.Values.ElementAt(i);
            }
            base.OnActionExecuting(filterContext);
        }
        private string Decrypt(string encryptedText)
        {
            try
            {
                byte[] keyData = Encoding.UTF8.GetBytes(_keyString.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] textData = GetBytes(encryptedText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms,
                  des.CreateDecryptor(keyData, _keyBytes), CryptoStreamMode.Write);
                cs.Write(textData, 0, textData.Length);
                cs.FlushFinalBlock();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        protected string GetString(byte[] data)
        {
            StringBuilder results = new StringBuilder();

            foreach (byte b in data)
                results.Append(b.ToString("X2"));

            return results.ToString();
        }
        protected byte[] GetBytes(string data)
        {
            byte[] results = new byte[data.Length / 2];

            for (int i = 0; i < data.Length; i += 2)
                results[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);

            return results;
        }
    }
}