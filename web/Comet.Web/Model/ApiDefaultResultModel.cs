namespace Comet.Web.Model
{
    public abstract class ApiDefaultResultModel
    {
        public int StatusCode { get; set; } = 200;
        public string ErrorMessage { get; set; } = null;
    }
}
