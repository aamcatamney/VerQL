using System.Collections.Generic;
using System.Linq;
using VerQL.Core.Models;

namespace VerQL.Core.Loaders
{
    public class LoaderResponse
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Database Database { get; set; }
        public bool Successful { get { return !this.Errors.Any(); } }
    }
}