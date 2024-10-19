namespace Tessera.Web.Services
{
    public interface IViewService
    {
        bool ShowChapterAddMenu { get; set; }
        bool ShowChapterSettings { get; set; }
        bool ShowTemplateMenu { get; set; }
        Type DynamicComponentType { get; set; }
        Dictionary<string, object> Parameters { get; set; }
        string ButtonIdGen();
    }

    public class ViewService : IViewService
    {
        public bool ShowChapterAddMenu { get; set; } = false;
        public bool ShowChapterSettings { get; set; } = false;
        public bool ShowTemplateMenu { get; set; } = false;
        public int ButtonId { get; set; } = 1;
        public Type DynamicComponentType { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public string ButtonIdGen()
        {
            return ButtonId++.ToString();
        }
    }

}
