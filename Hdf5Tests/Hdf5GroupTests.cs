﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hdf5DotNetTools;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HDF.PInvoke;

namespace Hdf5UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void WriteAndReadGroupsWithDataset()
        {
            string filename = Path.Combine(folder, "testGroups.H5");

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var dset = dsets.First();

                var groupId = H5G.create(fileId, "/A"); ///B/C/D/E/F/G/H
                Hdf5.WriteDataset(groupId, "test", dset);
                var subGroupId = Hdf5.CreateGroup(groupId, "C");
                var subGroupId2 = Hdf5.CreateGroup(groupId, "/D"); // will be saved at the root location 
                dset = dsets.Skip(1).First();
                Hdf5.WriteDataset(subGroupId, "test2", dset);
                Hdf5.CloseGroup(subGroupId);
                Hdf5.CloseGroup(subGroupId2);
                Hdf5.CloseGroup(groupId);
                groupId = H5G.create(fileId, "/A/B"); ///B/C/D/E/F/G/H
                dset = dsets.Skip(1).First();
                Hdf5.WriteDataset(groupId, "test", dset);
                Hdf5.CloseGroup(groupId);

                groupId = Hdf5.CreateGroupRecursively(fileId, "A/B/C/D/E/F/I");
                Hdf5.CloseGroup(groupId);
                Hdf5.CloseFile(fileId);


                fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                groupId = H5G.open(fileId, "/A/B");
                double[,] dset2 = (double[,])Hdf5.ReadDataset<double>(groupId, "test");
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5.CloseGroup(groupId) >= 0);
                groupId = H5G.open(fileId, "/A/C");
                dset2 = (double[,])Hdf5.ReadDataset<double>(groupId, "test2");
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5.CloseGroup(groupId) >= 0);
                bool same = dset == dset2;
                dset = dsets.First();
                dset2 = (double[,])Hdf5.ReadDataset<double>(fileId, "/A/test");
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5.GroupExists(fileId, "A/B/C/D/E/F/I"));

                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);

            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadGroupsInGroup()
        {
            string filename = Path.Combine(folder, "testGroupWithGroups.H5");

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = H5G.create(fileId, "/A");
                var subGroupId1 = Hdf5.CreateGroup(groupId, "B");
                var subGroupId2 = Hdf5.CreateGroup(groupId, "C");
                Hdf5.CloseGroup(groupId);
                Hdf5.CloseGroup(subGroupId1);
                Hdf5.CloseGroup(subGroupId2);
                Hdf5.CloseFile(fileId);

                fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                Assert.IsTrue(Hdf5.GroupExists(fileId, "A/B"));
                Assert.IsTrue(Hdf5.GroupExists(fileId, "A/C"));
                groupId = H5G.open(fileId, "/A");
                string[] groups = Hdf5.GroupGroups(groupId);
                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
