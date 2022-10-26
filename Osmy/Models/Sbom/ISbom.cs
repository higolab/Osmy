using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    internal interface ISbom
    {
        // TODO パッケージの依存関係等を取得できるようにする

        public string Name { get; set; }
    }
}
