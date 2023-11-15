using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSXFilterRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VSXFilterRepair.Tests
{
    [TestClass()]
    public class VSXFilterRepair_Tests
    {
        [TestMethod()]
        public void CheckFileValidityTest()
        {
            VSXFilterRepair repair = new VSXFilterRepair();
            Assert.IsTrue(repair.CheckFileValidity(@"../../Resources\Unit_Test_Sample.filters"), "Something is wrong.");
        }
    }
}