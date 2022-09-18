using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Poltergeist.Common.Utilities.Webs;

public class FatchHelper
{
    private string URL;
    public (string Address, int Port) Proxy;

    public FatchHelper(string url)
    {
        URL = url;
    }

    public HttpStatusCode GetStatusCode(bool allowAutoRedirect = false)
    {
        HttpStatusCode code = default;

        do
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(URL);
                req.AllowAutoRedirect = allowAutoRedirect;
                req.Timeout = 300000;
                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    code = res.StatusCode;
                }
            }
            catch
            {
                code = default;
            }
        } while (code == default);

        return code;
    }

    private WebClient GetClient()
    {
        var client = new WebClient()
        {
            Encoding = Encoding.Unicode
        };
        return client;
    }

    public string GetSourceCode()
    {

        using var client = GetClient();
        string sourceCode;
        do
        {
            try
            {
                var data = client.DownloadData(URL);
                sourceCode = Encoding.UTF8.GetString(data.ToArray());
            }
            catch
            {
                sourceCode = "";
            }
        } while (sourceCode.Length == 0);

        return sourceCode;
    }

    public byte[] Download()
    {
        using var client = GetClient();
        while (true)
        {
            try
            {
                var data = client.DownloadData(URL);
                return data;
            }
            catch
            {
            }
        }

        //return null;
    }

    public string GetJson()
    {
        using var client = GetClient();
        var data = client.DownloadData(URL);
        var text = Encoding.UTF8.GetString(data);
        return text;
    }
}
