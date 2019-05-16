using Coldairarrow.DataRepository;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Coldairarrow.UnitTests
{
    [TestClass]
    public class DynamicEntityTest
    {
        #region 构造函数

        static DynamicEntityTest()
        {
            for (int i = 1; i <= 100; i++)
            {
                Base_UnitTest newData = new Base_UnitTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Age = i,
                    UserId = "Admin" + i,
                    UserName = "超级管理员" + i
                };
                _dataList.Add(newData);
            }
        }

        public DynamicEntityTest()
        {
            _db.DeleteAll<Base_UnitTest>();
        }

        #endregion

        #region 私有成员

        private IRepository _db { get; } = DbFactory.GetRepository();
        private static Base_UnitTest _newData { get; } = new Base_UnitTest
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "Admin",
            UserName = "超级管理员",
            Age = 22
        };

        private static List<Base_UnitTest> _insertList { get; } = new List<Base_UnitTest>
        {
            new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "Admin1",
                UserName = "超级管理员1",
                Age = 22
            },
            new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "Admin2",
                UserName = "超级管理员2",
                Age = 22
            }
        };

        private static List<Base_UnitTest> _dataList { get; } = new List<Base_UnitTest>();

        private Type MapTable(Type absTable, string targetTableName)
        {
            return ShardingHelper.MapTable(absTable, targetTableName);
        }

        #endregion

        #region 测试用例

        [TestMethod]
        public void TransactionTest()
        {
            Type targetType = MapTable(typeof(Base_UnitTest), "Base_UnitTest_0");
            //失败事务,Base_UnitTest成功,Base_UnitTest_0失败
            Clear();
            using (var transaction = _db.BeginTransaction())
            {
                //Base_UnitTest成功
                new Action(() =>
                {
                    _db.ExecuteSql("insert into Base_UnitTest(Id) values('10') ");

                    _db.Insert(_newData);

                    var newData2 = _newData.DeepClone();
                    newData2.Id = Guid.NewGuid().ToSequentialGuid();
                    newData2.UserId = Guid.NewGuid().ToSequentialGuid();
                    _db.Insert(newData2);
                })();
                throw new Exception("111");
                //Base_UnitTest_0失败
                new Action(() =>
                {
                    _db.ExecuteSql("insert into Base_UnitTest_0(Id) values('10') ");

                    _db.Insert(_newData.ChangeType(targetType));

                    var newData2 = _newData.DeepClone();
                    newData2.Id = Guid.NewGuid().ToSequentialGuid();

                    //UserId唯一导致异常
                    //newData2.UserId = Guid.NewGuid().ToSequentialGuid();
                    _db.Insert(newData2.ChangeType(targetType));
                })();

                bool succcess = _db.EndTransaction().Success;
                Assert.AreEqual(succcess, false);
                Assert.AreEqual(_db.GetIQueryable<Base_UnitTest>().Count(), 0);
                Assert.AreEqual(_db.GetIQueryable(targetType).Count(), 0);
            }

            //成功事务,Base_UnitTest成功,Base_UnitTest_0成功
            Clear();
            using (var transaction = _db.BeginTransaction())
            {
                //Base_UnitTest成功
                new Action(() =>
                {
                    _db.ExecuteSql("insert into Base_UnitTest(Id) values('10') ");

                    _db.Insert(_newData);

                    var newData2 = _newData.DeepClone();
                    newData2.Id = Guid.NewGuid().ToSequentialGuid();
                    newData2.UserId = Guid.NewGuid().ToSequentialGuid();
                    _db.Insert(newData2);
                })();

                //Base_UnitTest_0成功
                new Action(() =>
                {
                    _db.ExecuteSql("insert into Base_UnitTest_0(Id) values('10') ");

                    _db.Insert(_newData.ChangeType(targetType));

                    var newData2 = _newData.DeepClone();
                    newData2.Id = Guid.NewGuid().ToSequentialGuid();
                    newData2.UserId = Guid.NewGuid().ToSequentialGuid();
                    _db.Insert(newData2.ChangeType(targetType));
                })();

                bool succcess = _db.EndTransaction().Success;
                Assert.AreEqual(succcess, true);
                Assert.AreEqual(_db.GetIQueryable<Base_UnitTest>().Count(), 3);
                Assert.AreEqual(_db.GetIQueryable(targetType).Count(), 3);
            }

            void Clear()
            {
                _db.DeleteAll<Base_UnitTest>();
                _db.DeleteAll(targetType);
            }
        }

        #endregion
    }
}
