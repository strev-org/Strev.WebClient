namespace Strev.WebClient
{
    public static class WebClientService
    {
        public static IWebClientService GetWebClientService() => new Service.WebClientService();
    }
}