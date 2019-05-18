using Coldairarrow.DataRepository;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Coldairarrow.UnitTests
{
    [TestClass]
    public class ShardingTest : BaseTest
    {
        static ShardingTest()
        {
            ShardingConfigBootstrapper.Bootstrap()
                //添加数据源
                .AddDataSource("BaseDb", DatabaseType.SqlServer, dbBuilder =>
                {
                    //添加物理数据库
                    dbBuilder.AddPhsicDb("BaseDb", ReadWriteType.ReadAndWrite);
                })
                //添加抽象数据库
                .AddAbsDb("BaseDb", absTableBuilder =>
                {
                    //添加抽象数据表
                    absTableBuilder.AddAbsTable("Base_UnitTest", tableBuilder =>
                    {
                        //添加物理数据表
                        tableBuilder.AddPhsicTable("Base_UnitTest_0", "BaseDb");
                        tableBuilder.AddPhsicTable("Base_UnitTest_1", "BaseDb");
                        tableBuilder.AddPhsicTable("Base_UnitTest_2", "BaseDb");
                    }, new ModShardingRule("Base_UnitTest", "Id", 3));
                });
        }
        private IShardingRepository _db { get; } = DbFactory.GetRepository().ToSharding();
        protected override void Clear()
        {
            _db.DeleteAll<Base_UnitTest>();
        }

        /// <summary>
        /// 插入数据测试
        /// </summary>
        [TestMethod]
        public void InsertTest()
        {
            //单条数据
            _db.Insert(_newData);
            var theData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());

            //多条数据
            Clear();
            _db.Insert(_insertList);
            var theList = _db.GetList<Base_UnitTest>();
            Assert.AreEqual(_insertList.OrderBy(X => X.Id).ToJson(), theList.OrderBy(X => X.Id).ToJson());
        }

        /// <summary>
        /// 删除数据测试
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            //删除表所有数据
            _db.Insert(_insertList);
            _db.DeleteAll<Base_UnitTest>();
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);

            //删除单条数据,对象形式
            Clear();
            _db.Insert(_newData);
            _db.Delete(_newData);
            count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);

            //删除多条数据
            Clear();
            _db.Insert(_insertList);
            _db.Delete(_insertList);
            count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);

            //删除指定数据
            Clear();
            _db.Insert(_insertList);
            _db.Delete<Base_UnitTest>(x => x.UserId == "Admin2");
            count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// 更新数据测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            //更新单条数据
            _db.Insert(_newData);
            var updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _db.Update(updateData);
            var dbUpdateData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());

            //更新多条数据
            Clear();
            _db.Insert(_insertList);
            var updateList = _insertList.DeepClone();
            updateList[0].UserId = "Admin3";
            updateList[1].UserId = "Admin4";
            _db.Update(updateList);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
            Assert.AreEqual(2, count);

            //更新单条数据指定属性
            Clear();
            _db.Insert(_newData);
            var newUpdateData = _newData.DeepClone();
            newUpdateData.UserName = "普通管理员";
            newUpdateData.UserId = "xiaoming";
            newUpdateData.Age = 100;
            _db.UpdateAny(newUpdateData, new List<string> { "UserName", "Age" });
            var dbSingleData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            newUpdateData.UserId = "Admin";
            Assert.AreEqual(newUpdateData.ToJson(), dbSingleData.ToJson());

            //更新多条数据指定属性
            Clear();
            _db.Insert(_insertList);
            var newList1 = _insertList.DeepClone();
            var newList2 = _insertList.DeepClone();
            newList1.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserId = "Test";
                aData.UserName = "测试";
            });
            newList2.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserName = "测试";
            });

            _db.UpdateAny(newList1, new List<string> { "UserName", "Age" });
            var dbData = _db.GetList<Base_UnitTest>();
            Assert.AreEqual(newList2.OrderBy(x => x.Id).ToJson(), dbData.OrderBy(x => x.Id).ToJson());

            //更新指定条件数据
            Clear();
            _db.Insert(_newData);
            _db.UpdateWhere<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIShardingQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        /// <summary>
        /// 查找数据测试
        /// </summary>
        [TestMethod]
        public void SearchTest()
        {
            //GetList获取表的所有数据
            _db.Insert(_insertList);
            var dbList = _db.GetList<Base_UnitTest>();
            Assert.AreEqual(_insertList.OrderBy(x => x.Id).ToJson(), dbList.OrderBy(x => x.Id).ToJson());

            //GetIQueryable获取实体对应的表，延迟加载，主要用于支持Linq查询操作
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin1").Count();
            Assert.AreEqual(1, count);

            //GetIQPagination获取分页后的数据
            Clear();
            _db.Insert(_dataList);
            Pagination pagination = new Pagination
            {
                SortField = "Age",
                SortType = "asc",
                PageIndex = 2,
                PageRows = 20
            };
            dbList = _db.GetIShardingQueryable<Base_UnitTest>().GetPagination(pagination);
            var dataList = _dataList.GetPagination(pagination);
            Assert.AreEqual(dbList.ToJson(), dataList.ToJson());
        }
    }
}
