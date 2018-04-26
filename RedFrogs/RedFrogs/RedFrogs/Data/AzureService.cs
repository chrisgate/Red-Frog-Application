﻿using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Plugin.Connectivity;
using RedFrogs.Data;
using RedFrogs.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(AzureService))]
namespace RedFrogs.Data
{
    public class AzureService
    {
        public MobileServiceClient Client { get; set; } = null;
        IMobileServiceSyncTable<Events> eventsTable;
        IMobileServiceSyncTable<FeedBack> feedbackTable;
        IMobileServiceSyncTable<CaseInfo> caseInfoTable;

        public async Task Initialize()
        {
            if (Client?.SyncContext?.IsInitialized ?? false)
                return;

            var appUrl = "https://redfrogtest.azurewebsites.net";

            Client = new MobileServiceClient(appUrl);

            //InitialzeDatabase for path
            var path = "syncstore.db";
            path = Path.Combine(MobileServiceClient.DefaultDatabasePath, path);

            //setup our local sqlite store and intialize our table
            var store = new MobileServiceSQLiteStore(path);

            //Define table
            store.DefineTable<Events>();
            store.DefineTable<FeedBack>();
            store.DefineTable<CaseInfo>();

            //Initialize SyncContext
            await Client.SyncContext.InitializeAsync(store);

            //Get our sync table that will call out to azure
            eventsTable = Client.GetSyncTable<Events>();
            feedbackTable = Client.GetSyncTable<FeedBack>();
            caseInfoTable = Client.GetSyncTable<CaseInfo>();
        }

        public async Task SyncEvents()
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                    return;

                await eventsTable.PullAsync("allEvents", eventsTable.CreateQuery());
                
                await Client.SyncContext.PushAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync events, that is alright as we have offline capabilities: " + ex);
            }
        }

        public async Task<IEnumerable<Events>> GetEvents()
        {
            //Initialize & Sync
            await Initialize();
            await SyncEvents();

            return await eventsTable.Where(e => e.IsClosed == 0).ToEnumerableAsync(); ;

        }

        public async Task AddEvent(Events toAdd)
        {
            await Initialize();
            await eventsTable.InsertAsync(toAdd);
            await SyncEvents();
        }

        public async Task UpdateEvent(Events toUpdate)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                    return;

                await Initialize();
                toUpdate.IsClosed = 1;
                await eventsTable.UpdateAsync(toUpdate);
                await SyncEvents();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to sync events: " + ex);
            }
        }

        public async Task AddFeedback(FeedBack feedback)
        {
            await Initialize();
            await feedbackTable.InsertAsync(feedback);
            await Client.SyncContext.PushAsync();
        }

        public async Task AddCaseInfo(CaseInfo caseInfo)
        {
            await Initialize();
            await caseInfoTable.InsertAsync(caseInfo);
        }

        public async Task<IEnumerable<CaseInfo>> GetEventCases(string eventName)
        {
            //Initialize
            await Initialize();

            return await caseInfoTable.Where(e => e.EventName == eventName).ToEnumerableAsync(); ;

        }

    }
}
