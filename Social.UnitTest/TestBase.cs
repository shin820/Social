using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.UnitTest
{
    public class TestBase
    {
        static TestBase()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(new[] { "Social.Application" }));
        }
    }
}
