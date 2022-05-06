/***********************************************
 * 功能：封装Adapter、DataSet/DataTable、BindingSource三大模组，打通数据库、内存数据、控件三者的之间的交互
 * 构建目标：理顺数据库、内存数据与控件三者之间的关系，打通三者之间的连接
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-1-8
 * 修改信息：
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EckyStudio.M.BaseModel.db
{
    public class DbAdapterWrapper
    {
        DbDataAdapter mAdapter;
        DataTable mTable = new DataTable();
        BindingSource mSrc = new BindingSource();

        public DbAdapterWrapper(DbDataAdapter adapter) {
            mAdapter = adapter;

            mAdapter.Fill(mTable);
            mSrc.DataSource = mTable;
        }

        

        /// <summary>
        /// 获取数据源
        /// </summary>
        public BindingSource DataSrc {
            get { return mSrc; }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void Update() {
            mAdapter.Update(mTable);//同步到数据库                           
        }



        /// <summary>
        /// 刷新数据，从数据库重新获取数据
        /// </summary>
        public void Refresh() {
            mTable.Clear();
            mAdapter.Fill(mTable);
            mTable.AcceptChanges();//数据状态复位
        }

        /// <summary>
        /// 判断是否数据是否有变更
        /// </summary>
        public bool HasChanges
        {
            get {
                mSrc.EndEdit();
                if (mTable.GetChanges() == null)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 向内存关联表DataTable添加一行，添加后，调用Update直接能更新至数据库
        /// </summary>
        /// <returns></returns>
        public DataRow NewRow() {
            //return mTable.NewRow();
            DataRowView drv = (DataRowView)mSrc.AddNew();
            mSrc.EndEdit();//让新增行马上起效
            return drv.Row;
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose() {
            mAdapter.Dispose();
            mTable.Dispose();
            mSrc.Dispose();
        }

        //public DataTableMapping GetTableMapping() {
        //    int count = mAdapter.TableMappings.Count;
        //    return mAdapter.TableMappings[0];
        //}
    }
}
