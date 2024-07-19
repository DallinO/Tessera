using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Core.Services
{
    public interface IViewService
    {
        bool ShowChapterAddMenu { get; set; }
        bool ShowChapterSettings { get; set; }
        bool ShowTemplateMenu { get; set; }
    }

    public class ViewService : IViewService
    {
        public bool ShowChapterAddMenu { get; set; } = false;
        public bool ShowChapterSettings { get; set; } = false;
        public bool ShowTemplateMenu { get; set; } = false;
    }

}
