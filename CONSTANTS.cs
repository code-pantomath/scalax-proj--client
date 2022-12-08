

namespace Scalax_client.CONSTANTS;

public class CONSTANTS
{
    private static readonly bool isDevEnv = false;

    public static readonly string exeFilePath = Environment.CurrentDirectory;
    public static readonly string tmpFilePath = Path.Combine(exeFilePath, "tmp.txt");

    public static readonly string SERVER_ENDPOINT_URL = isDevEnv ? "http://127.0.0.1:5000" : "https://site--scalax-server-01--n5gfpf86nbvs.code.run";
}
