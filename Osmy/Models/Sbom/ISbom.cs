using Osmy.Views;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    public interface ISoftware
    {
        string Name { get; set; }

        ISbom? LatestSbom { get; }

        List<ISbom> Sboms { get; }
    }

    public class Software : ISoftware
    {
        public string Name { get; set; }

        public ISbom? LatestSbom { get; set; }

        public List<ISbom> Sboms { get; set; }

        public Software(string name, IEnumerable<ISbom> sboms, ISbom latestSbom)
        {
            Name = name;
            Sboms = sboms.ToList();
            LatestSbom = latestSbom;
        }

        public Software(string name)
        {
            Name = name;
            Sboms = new List<ISbom>();
        }

        // TODO
        public Software(string name, string sbomFile)
        {
            Name = name;
            LatestSbom = new Spdx(sbomFile);
            Sboms = new List<ISbom>() { LatestSbom };
        }
    }

    public interface ISbom
    {
        /// <summary>
        /// byte array of SBOM document data
        /// </summary>
        //byte[] RawData { get; set; }

        IPackage RootPackage { get; }

        IPackage[] Packages { get; }

        DependencyGraph DependencyGraph { get; }
    }

    public interface IPackage
    {
        string Name { get; }
        string Version { get; }
        bool IsRootPackage { get; }
    }
}
