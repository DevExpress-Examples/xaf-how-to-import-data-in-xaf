using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportData.Module {
    public delegate void ImportDataDelegate<T>(IObjectSpace objectSpace, object masterObject);
}
