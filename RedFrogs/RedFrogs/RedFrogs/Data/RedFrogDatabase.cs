﻿using RedFrogs.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedFrogs.Data
{
    public class RedFrogDatabase
    {
        readonly SQLiteAsyncConnection conn;

        public RedFrogDatabase(string dbPath)
        {
            conn = new SQLiteAsyncConnection(dbPath);
            conn.CreateTableAsync<Events>();
            
        }

        public Task<List<Events>> GetAllEvents()
        {
            return conn.QueryAsync<Events>("Select * FROM [Events]");
        }

        public Task<Events> GetEvent(string id)
        {
            return conn.Table<Events>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveAllEvents(List<Events> toSave)
        {
            return conn.InsertAllAsync(toSave);
        }

        public void DeleteAllEvents()
        {
            conn.ExecuteAsync("DELETE FROM Events");
            
        }

        public Task<List<FeedBack>> GetAllFeedBack()
        {
            return conn.QueryAsync<FeedBack>("Select * FROM [FeedBack]");
        }

        public Task<List<CaseInfo>> GetAllCaseInfo()
        {
            return conn.QueryAsync<CaseInfo>("Select * FROM [CaseInfo]");
        }

        public Task<CaseInfo> getCaseInfo(int id)
        {
            return conn.Table<CaseInfo>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<List<Symptoms>> GetAllSymptoms()
        {
            return conn.QueryAsync<Symptoms>("Select * FROM [Symptoms]");
        }       

        /*public Task<FeedBack> GetFeedback(int id)
        {
            return conn.Table<FeedBack>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }*/

        public Task<Symptoms> getSymptom(int id)
        {
            return conn.Table<Symptoms>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveCaseInfo(CaseInfo item)
        {
            if (item.ID != 0)
            {
                return conn.UpdateAsync(item);
            }
            else
            {
                return conn.InsertAsync(item);
            }
        }
    }
}
